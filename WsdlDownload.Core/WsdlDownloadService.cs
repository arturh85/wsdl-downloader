namespace WsdlDownload.Core;

using System.Net.Http;
using System.Xml;
public class WsdlDownloadService
{
    private readonly HttpClient httpClient;

    public WsdlDownloadService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task DownloadWsdlRecursive(string xmlUrl, string xmlPath)
    {
        if (File.Exists(xmlPath))
        {
            return;
        } 
        var fetchHistory = new HashSet<string>();
        var wsdlStream = await httpClient.GetStreamAsync(xmlUrl);
        var (xmlDoc, nsManager) = XmlUtility.LoadXsdStream(wsdlStream);
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
        var xsdStream = await httpClient.GetStreamAsync(url);
        var (xmlDoc, nsManager) = XmlUtility.LoadXsdStream(xsdStream);
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
