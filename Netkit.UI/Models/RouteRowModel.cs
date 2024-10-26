using NetKit.Lib.IP;

namespace NetKit.UI.Models;

public class RouteRowModel(RouteTableRow routeTableRow)
{
    public string? Destination { get; set; } = routeTableRow.Destination?.ToString();
    public string? Mask { get; set; } = routeTableRow.Mask?.ToString();
    public string Policy { get; set; } = routeTableRow.Policy.ToString();
    public string? NextHop { get; set; } = routeTableRow.NextHop?.ToString();
    public string IfIndex { get; set; } = routeTableRow.IfIndex.ToString();
    public string Type { get; set; } = routeTableRow.Type.ToString();
    public string Proto { get; set; } = routeTableRow.Proto.ToString();
    public string Age { get; set; } = routeTableRow.Age.ToString();
    public string NextHopAs { get; set; } = routeTableRow.NextHopAs.ToString();
    public string Metric1 { get; set; } = routeTableRow.Metric1.ToString();
    public string Metric2 { get; set; } = routeTableRow.Metric2.ToString();
    public string Metric3 { get; set; } = routeTableRow.Metric3.ToString();
    public string Metric4 { get; set; } = routeTableRow.Metric4.ToString();
    public string Metric5 { get; set; } = routeTableRow.Metric5.ToString();
}