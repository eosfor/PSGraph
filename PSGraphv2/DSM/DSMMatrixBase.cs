using MathNet.Numerics.LinearAlgebra;
using PSGraph.Model;
using QuikGraph;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Data.Text;
using Svg;


namespace PSGraph.DesignStructureMatrix
{
    public class DsmMatrixBase : IDsmMatrix, IDsmMatrixView
    {
        protected Matrix<Single> _dsm;

        protected Dictionary<PSVertex, int> _rowIndex = new Dictionary<PSVertex, int>();
        protected Dictionary<PSVertex, int> _colIndex = new Dictionary<PSVertex, int>();

        public Dictionary<PSVertex, int> RowIndex { get => _rowIndex; }
        public Dictionary<PSVertex, int> ColIndex { get => _colIndex; }

        public Matrix<Single> Dsm { get => _dsm; }

        public Single this[PSVertex from, PSVertex to] { get => GetElementByVertex(from, to); set => SetElementByVertex(from, to, value); }
        public Single this[int row, int column] { get => _dsm[row, column]; set => _dsm[row, column] = value; }

        private void SetElementByVertex(PSVertex row, PSVertex column, float value)
        {
            _dsm[_rowIndex[row], _colIndex[column]] = value;
        }

        private float GetElementByVertex(PSVertex from, PSVertex to)
        {
            return _dsm[_rowIndex[from], _colIndex[to]];
        }

        private Matrix<float>? GraphToDSM(PSBidirectionalGraph graph)
        {
            Matrix<Single> dsm = Matrix<Single>.Build.Dense(graph.VertexCount, graph.VertexCount);

            int idx = 0;
            foreach (var item in graph.Vertices)
            {
                _colIndex[item] = idx;
                _rowIndex[item] = idx;
                idx++;
            }

            foreach (var e in graph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                // TODO: support tag as weight?;

                var colIndex = _colIndex[to];
                var rowIndex = _rowIndex[from];
                dsm[rowIndex, colIndex] = linkWeight;
            }

            return dsm;
        }

        public PSBidirectionalGraph GraphFromDSM()
        {
            var ret = new PSBidirectionalGraph();

            foreach (var row in _rowIndex)
            {
                foreach (var col in _colIndex)
                {
                    if (this[row.Key, col.Key] > 0)
                    {
                        ret.AddVerticesAndEdge(new PSEdge(row.Key, col.Key, new PSEdgeTag()));
                    }
                }
            }

            return ret;

        }

        public IDsmMatrix Power(int exponent)
        {
            var retDsm = new DsmMatrixBase(this);
            retDsm._dsm = this._dsm.Power(exponent);

            return retDsm;
        }

        public virtual SvgDocument ToSvg()
        {
            int itemSize = 45; // the size of a cell of a matrix
            int h = _dsm.ColumnCount * itemSize + itemSize;
            int w = _dsm.RowCount * itemSize + itemSize;

            var svgDoc = new SvgDocument()
            {
                Width = w,
                Height = h
            };

            GenerateMatrixViewAnnotations(itemSize, svgDoc);
            GenerateMatrixView(itemSize, svgDoc);
            return svgDoc;
        }

        private void GenerateMatrixViewAnnotations(int itemSize, SvgDocument svgDoc)
        {
            int y = 0;
            int x = 0;

            foreach (var item in _rowIndex)
            {
                var el = new Svg.SvgText(item.Key.ToString());
                x = itemSize - 15; // magic number as FontSize is not a square
                el.X.Add(x);
                el.Y.Add((y + 1) * itemSize + (int)itemSize/2);
                svgDoc.Children.Add(el);
                y++;
            }

            x = 0;
            y = 0;
            foreach (var item in _colIndex)
            {
                var el = new Svg.SvgText(item.Key.ToString());
                y = itemSize - (int)el.FontSize - 5;
                el.X.Add((x + 1) * itemSize + (int)itemSize/2);
                el.Y.Add( y);
                svgDoc.Children.Add(el);
                x++;
            }
        }
        private void GenerateMatrixView(int itemSize, SvgDocument svgDoc)
        {
            for (int i = 0; i < _dsm.ColumnCount; i++)
            {
                var x = i * itemSize + itemSize;
                for (int j = 0; j < _dsm.RowCount; j++)
                {
                    var y = j * itemSize + itemSize;
                    var rect = new SvgRectangle()
                    {
                        Width = itemSize,
                        Height = itemSize,
                        X = x,
                        Y = y,
                        StrokeWidth = (float)0.5,
                        Stroke = new SvgColourServer(System.Drawing.Color.DimGray),
                        Fill = FillColor(i, j)
                    };
                    svgDoc.Children.Add(rect);
                }

            }

            SvgColourServer FillColor(int i, int j)
            {
                if (i == j) return new SvgColourServer(System.Drawing.Color.DarkGray);
                return _dsm[j, i] == 1 ? new SvgColourServer(System.Drawing.Color.SlateBlue) : new SvgColourServer(System.Drawing.Color.White);
            }
        }

        public void ExportText(string Path)
        {
            DelimitedWriter.Write(Path, _dsm, ",");
        }

        #region constructors
        public DsmMatrixBase(PSBidirectionalGraph graph)
        {
            _dsm = GraphToDSM(graph);
        }

        public DsmMatrixBase(DsmMatrixBase dsm)
        {
            this._dsm = Matrix<Single>.Build.Dense(dsm._dsm.RowCount, dsm._dsm.ColumnCount);
            dsm._dsm.CopyTo(this._dsm);
            _rowIndex = new Dictionary<PSVertex, int>(dsm._rowIndex);
            _colIndex = new Dictionary<PSVertex, int>(dsm._colIndex);
        }

        public DsmMatrixBase(int rows, int cols)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = new Dictionary<PSVertex, int>();
            _colIndex = new Dictionary<PSVertex, int>();
        }

        public DsmMatrixBase(int rows, int cols, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
        {
            _dsm = Matrix<Single>.Build.Dense(rows, cols);
            _rowIndex = rowIndex;
            _colIndex = colIndex;
        }

        public DsmMatrixBase(PSBidirectionalGraph graph, Dictionary<PSVertex, int> rowIndex, Dictionary<PSVertex, int> colIndex)
        {
            _rowIndex = rowIndex;
            _colIndex = colIndex;

            _dsm = Matrix<Single>.Build.Dense(graph.VertexCount, graph.VertexCount);

            foreach (var e in graph.Edges)
            {
                var from = e.Source;
                var to = e.Target;
                var linkWeight = 1;

                this[_rowIndex[from], _colIndex[to]] = linkWeight;
            }
        }

        #endregion constructors
    }
}