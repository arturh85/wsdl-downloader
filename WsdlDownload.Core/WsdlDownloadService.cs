namespace WsdlDownload.Core;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
public class WsdlDownloadService
{
    private readonly HttpClient httpClient;

    public WsdlDownloadService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<(XmlDocument? xmlDoc, XmlNamespaceManager? nsManager)> DownloadWsdlBase(string xmlUrl, string xmlPath, string? username = null, string? password = null)
    {
        if (File.Exists(xmlPath))
        {
            return (null, null);
        }

        if (username != null && password != null)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", $"{username}:{password}".Base64Encode());
        }

        return await httpClient.LoadXsdStream(xmlUrl);
    }

    public async Task<int> DownloadWsdls(string csvInputFile, string outputFolderPath)
    {
        if (!File.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string[] lines = File.ReadAllLines(csvInputFile);
        // url; username; password; filename;
        var wsdls = lines.Select(s => s.Split(';'));

        foreach (var wsdl in wsdls){
            string outputFile = Path.Combine(outputFolderPath, (string.IsNullOrEmpty(wsdl[3]) ? Guid.NewGuid().ToString() : wsdl[3]) + ".wsdl");
            await DownloadWsdl(wsdl[0], outputFile, wsdl[1], wsdl[2]);
        }

        return lines.Length;
    }

    public async Task DownloadWsdl(string xmlUrl, string xmlPath, string? username = null, string? password = null)
    {
        var (xmlDoc, _) = await DownloadWsdlBase(xmlUrl, xmlPath, username, password);
        if (xmlDoc == null)
        {
            return;
        }
        xmlDoc.Save(xmlPath);
    }

    public async Task DownloadWsdlRecursive(string xmlUrl, string xmlPath)
    {
        var (xmlDoc, nsManager) = await DownloadWsdlBase(xmlUrl, xmlPath);
        if (xmlDoc == null || nsManager == null)
        {
            return;
        }

        var fetchHistory = new HashSet<string>();

        // xsd:import is used in wsdl files
        // xsd:include is used in xsd files
        var importNodes =
            from item in xmlDoc.SelectNodes("//xsd:import | //xsd:include", nsManager)!.OfType<XmlElement>()
            select item;
        foreach (var node in importNodes.ToList())
        {
            var parent = (node.ParentNode as XmlElement)!;
            if (!parent.HasAttribute("targetNamespace")) {
                parent.SetAttribute("targetNamespace", node.GetAttribute("namespace"));
            }
            await WalkXsdRecursive(fetchHistory, node, parent);
            parent.RemoveChild(node);
        }
        xmlDoc.Save(xmlPath);
    }

    // Walks over xsd:include tags, downloads the xsd and replaces the include with its contents and repeats this recursivly
    async Task WalkXsdRecursive(HashSet<string> fetchHistory, XmlElement importNode, XmlElement targetNode)
    {
        var url = importNode.GetAttribute("schemaLocation");
        if (fetchHistory.Contains(url))
        {
            return;
        }
        fetchHistory.Add(url);
        var (xmlDoc, nsManager) = await httpClient.LoadXsdStream(url);
        foreach (var child in xmlDoc.SelectSingleNode("/xsd:schema", nsManager)!.ChildNodes.OfType<XmlElement>())
        {
            if (child.Name == "xsd:include") { 
                await WalkXsdRecursive(fetchHistory, child, targetNode);
            } 
            else
            {
                XmlNode newNode = targetNode.OwnerDocument.ImportNode(child, true);
                targetNode.AppendChild(newNode);
            }
        }
    }
}
