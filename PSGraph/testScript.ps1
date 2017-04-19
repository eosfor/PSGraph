Import-Module  "C:\Projects\Repos\Public\PSGraph\PSGraph\bin\Debug\PSGraph.psd1"

$subs = Import-Clixml "C:\tests\objects\subs.xml" #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allVnets =  Import-Clixml -Path C:\tests\objects\vnets.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allGWs =  Import-Clixml -Path C:\tests\objects\gws.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allConections =  Import-Clixml -Path C:\tests\objects\connections.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$allCircuits =  Import-Clixml -Path C:\tests\objects\circuits.xml #| ? subscriptionid -eq 'b37b6a13-1f1c-48e7-8f89-d5859181c926'
$classicVnets = Import-Clixml "C:\tests\objects\classicNets.xml"

class ClassicVNET : Psgraph.PSGraphVertex {
    [string]$SubscriptionID
    [string]$Name
    [string]$ResourceType
    [string]$ResourceGroupName
    [string]$Location
    [array]$Subnets
    [string]$AddressSpace
    [string]$DHCPOptions
    [string]$PeeredNetwork
    [string]$VNETType = "ARM"
    [string]$ResourceID
    [string]$MPLSGWConnection
    [bool]Equals([System.Object]$y)  { return $this.IsTypeEqual($y) -AND  ($this.Label -eq ([ClassicVNET]$y).Label)}
    [int]GetHashCode() {return $this.label.gethashcode()}
}

class ClassicCircuit : Psgraph.PSGraphVertex {
    [string]$Name
    [string]$ResourceType
    [string]$SubscriptionID
    ClassicCircuit([string]$n, [string]$loc, [string]$sID){
        $this.name = $n
        $this.ResourceType = "ClassicMPLS"
        $this.SubscriptionID = $sID
        $this.Label = $n
        $this.Shape =  "Circle"
    }
    [bool]Equals([Object]$y)  { return $this.IsTypeEqual($y) -AND  ($this.Label -eq ([ClassicCircuit]$y).Label)}
    [int]GetHashCode() {return $this.label.gethashcode()}
}

$g = New-Graph -Type AdjacencyGraph

#add classic VNETS
foreach ($vnet in $classicVnets) {
    $vnetSite = $vnet.Netconfig.NetworkConfigXML.NetworkConfiguration.VirtualNetworkConfiguration.VirtualNetworkSites.VirtualNetworkSite
    if ($vnetSite) {
        $gwConnection = $vnetSite.Gateway.ConnectionsToLocalNetwork.LocalNetworkSiteRef
        if ($gwConnection) {
            $mplsConnection =
                $gwConnection | select -ExpandProperty Name
        }
        $vnetHash = [ClassicVNET]@{
            Label = $vnetSite.Name
            SubscriptionId = $vnet.SubscriptionId
            Name  =  $vnetSite.Name
            ResourceType = "ClassicNetwork"
            Location = $vnetSite.Location
            Subnets = $vnetSite.subnets
            AddressSpace = $vnetSite.addressSpace.addressPrefixes
            DHCPOptions = $vnetSite.Dns.DnsServers.DnsServer
            PeeredNetwork = "NA"
            ResourceID = "NA"
            MPLSGWConnection = $mplsConnection
        }
        Add-Vertex -Vertex $vnetHash -Graph $g
    }
}


#add classic circuits
foreach ($er in $classicVnets) {
    $vnetSite = $er.Netconfig.NetworkConfigXML.NetworkConfiguration.VirtualNetworkConfiguration.VirtualNetworkSites.VirtualNetworkSite
    if ($vnetSite) {
        $gwConnection = $vnetSite.Gateway.ConnectionsToLocalNetwork.LocalNetworkSiteRef
        if ($gwConnection) {
            $gwConnection | % {
                $circuitObject = [ClassicCircuit]::new($_.Name, $er.Location, $er.SubscriptionId)
                Add-Vertex -Vertex $circuitObject -Graph $g
                #$g.AddVertex($circuitObject)
            }
        }
    }
}


foreach ($cv in  ($g.vertices  | where {$_ -is [ClassicVNET]})){
    foreach ($ce in ($g.vertices  | where {$_ -is [ClassicCircuit]})){
        if ($cv.MPLSGWConnection -eq $ce.Name) {
            add-edge -from $cv -to $ce -graph $g
        }
    }
}