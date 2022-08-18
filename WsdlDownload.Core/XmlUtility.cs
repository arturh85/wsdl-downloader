namespace WsdlDownload.Core;

using System.Xml;

public static class XmlUtility
{
    public static (XmlDocument, XmlNamespaceManager) LoadXsdStream(Stream xml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xml);
        return (xmlDoc, CreateNsManager(xmlDoc));
    }

    // Adds xsd namespace which is required to make xmlDoc.SelectNodes("//xsd:import") work
    public static XmlNamespaceManager CreateNsManager(XmlDocument xmlDoc) {
        var nsManager = new XmlNamespaceManager(new NameTable());
        nsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
        return nsManager;
    }

    public static async Task<(XmlDocument xmlDoc, XmlNamespaceManager nsManager)> LoadXsdStream(this HttpClient httpClient, string xmlUrl)
    {
        var wsdlStream = await httpClient.GetStreamAsync(xmlUrl);
        var(xmlDoc, nsManager) = XmlUtility.LoadXsdStream(wsdlStream);
        return (xmlDoc, nsManager);
    }
    
}
