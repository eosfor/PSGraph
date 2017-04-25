Import-Module "C:\Repo\Github\PSGraph\PSGraph\bin\Debug\PSQuickGraph.psd1" -verbose

describe "PSGraph Pester test" {
	context "Create graph" {
		it "Creating graph"  {
			$g = New-Graph -Type AdjacencyGraph
			$g | Should Not BeNullOrEmpty
		}
	}
	context "Add vertices" {
		it "Adding vertices" {
			$g = New-Graph -Type AdjacencyGraph
            [char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

            $g.VertexCount | should Not BeNullOrEmpty
            $g.VertexCount | should Be 16
		}
        it "Uniqueness test: types are the same" {
			$g = New-Graph -Type AdjacencyGraph
            [char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null
            [char]'A'..[char]'P' | % { Add-Vertex -Vertex ([string]([char]$_)) -Graph $g } | Out-Null

            $g.VertexCount | should Not BeNullOrEmpty
            $g.VertexCount | should Be 16
        }
        it "Uniqueness test: types are different" {
			$g = New-Graph -Type AdjacencyGraph
            class testclass1 : Psgraph.PSGraphVertex {
                [string]$property1
                [string]$property2
                [string]get_UniqueKey() { return $this.property2 }
            }

            class testclass2 : Psgraph.PSGraphVertex {
                [string]$property3
                [string]$property4
                [string]get_UniqueKey() { return $this.property4 }
            }


            Add-Vertex -Vertex ([testclass1]@{property1 = 1; property2 = 'A'}) -Graph $g
            Add-Vertex -Vertex ([testclass2]@{property3 = 1; property4 = 'B'}) -Graph $g
            $g.VertexCount | should Not BeNullOrEmpty
            $g.VertexCount | should Be 2

            Add-Vertex -Vertex ([testclass1]@{property1 = 2; property2 = 'C'}) -Graph $g
            Add-Vertex -Vertex ([testclass2]@{property3 = 25; property4 = 'C'}) -Graph $g

            $g.VertexCount | should Not BeNullOrEmpty
            $g.VertexCount | should Be 3

        }
	}
}