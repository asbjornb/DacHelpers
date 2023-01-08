﻿using System.Xml;

namespace DacHelpers.DeployReporting;

/// <summary>
/// Represents a deployment report from a DACPAC deployment similar to what can be generated by DacServices.GenerateDeployReport or SqlPackage.exe /Action:GenerateDeployReport
/// </summary>
public class DeployReport
{
    /// <summary>
    /// List of the alerts in the deploy report
    /// </summary>
    public readonly List<Alert> Alerts = new();
    /// <summary>
    /// List of the operations in the deploy report
    /// </summary>
    public readonly List<SqlOperation> Operations = new();

    /// <summary>
    /// Constructor for deployreports. Not commonly used since the default is to construct via .Parse() static method.
    /// </summary>
    /// <param name="alerts">Alerts for the deploy report</param>
    /// <param name="operations">Operations for the deploy report</param>
    public DeployReport(List<Alert> alerts, List<SqlOperation> operations)
    {
        Alerts = alerts;
        Operations = operations;
    }

    /// <summary>
    /// Parses a deploy report from a DACPAC deployment xml
    /// </summary>
    /// <param name="xml">String containing xml</param>
    public static DeployReport Parse(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var root = doc.DocumentElement!;
        var alerts = new List<Alert>();
        var operations = new List<SqlOperation>();
        foreach (XmlElement element in root.GetElementsByTagName("Alert"))
        {
            var alertType = element.GetAttribute("Name");
            foreach (XmlElement issue in element.GetElementsByTagName("Issue"))
            {
                var issueValue = issue.GetAttribute("Value");
                alerts.Add(new Alert(alertType, issueValue));
            }
        }
        foreach (XmlElement element in root.GetElementsByTagName("Operation"))
        {
            var operationTypeString = element.GetAttribute("Name");

            if(Enum.TryParse<SqlOperationType>(operationTypeString, true, out var operationType))
            {
                foreach (XmlElement item in element.GetElementsByTagName("Item"))
                {
                    var objectName = item.GetAttribute("Value");
                    var objectType = item.GetAttribute("Type");
                    operations.Add(new SqlOperation(operationType, objectType, objectName));
                }
            }
        }
        return new DeployReport(alerts, operations);
    }
}