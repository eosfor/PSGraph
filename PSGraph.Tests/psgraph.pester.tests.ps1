describe "PSGraph Pester test" {
	context "Create graph  test" {
		it "Creating graph"  {
			$g = New-Graph -Type AdjacencyGraph -EnableVertexComparer
			$g | Should Not BeNullOrEmpty
		}
	}
	context "Test for uniqueness" {
		it "Test for uniqueness" {
			class vnet : Psgraph.PSGraphVertex {
				[string]$iprange;
				[string]$type = "vnet"
			}

			class ER : Psgraph.PSGraphVertex {
				[string]$ipaddress;
				[string]$type = "ER"
			}

			[ER]$er  = @{Label = "yyy";  ipaddress = "192.168.3.1"}
			[ER]$erEnd  = @{Label = "end";  ipaddress = "192.168.10.1"}

			$g = New-Graph -Type AdjacencyGraph -EnableVertexComparer
			Add-Edge -from ([vnet]@{Label = "aaa";  shape = "Parallelogram"; iprange = "192.168.1.0/24"}) -to $er -Graph $g | should be $true
			Add-Edge -from ([vnet]@{Label = "aaa";  shape = "Parallelogram"; iprange = "192.168.1.0/24"}) -to $er -Graph $g | should be $false
		}
	}
}