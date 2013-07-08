namespace SVD
{
    public class Syntez
    {
        #region Fields

        private double[,] _a;
        private double[,] _u;
        private double[,] _v;

        private double[,] _au1;
        private double[,] _au2;

        private double[,] _av1;
        private double[,] _av2;

        #endregion

        #region Properties

        public double[,] AU1
        {
            get { return _au1; }
        }

        public double[,] AU2
        {
            get { return _au2; }
        }

        public double[,] AV1
        {
            get { return _av1; }
        }

        public double[,] AV2
        {
            get { return _av2; }
        }

        #endregion

        #region Methods

        public Syntez(double[,] a, double[,] u, double[,] v)
        {
            _a = a;
            _u = u;
            _v = v;

            var mRow = _a.GetLength(0) +_u.GetLength(0);
            var nCol = _a.GetLength(1);// +_u.GetLength(1);

            _au1 = new double[mRow, nCol];
            _au2 = new double[mRow, nCol];

            mRow = _a.GetLength(0) +_v.GetLength(0);
            nCol = _a.GetLength(1);// +_v.GetLength(1);

            _av1 = new double[mRow, nCol];
            _av2 = new double[mRow, nCol];

            Run();
        }

        private void Run()
        {
            for (var iRow = 0; iRow < _a.GetLength(0); iRow++)
            {
                for (var jCol = 0; jCol < _a.GetLength(1); jCol++)
                {
                    AU1[iRow*2, jCol] = _a[iRow, jCol];
                    AU1[iRow*2 + 1, jCol] = _a[iRow, jCol] + _u[iRow, jCol];
                }
            }

            for (var iRow = 0; iRow < _v.GetLength(0); iRow++)
            {
                for (var jCol = 0; jCol < _v.GetLength(1); jCol++)
                {
                    AV1[iRow * 2, jCol] = _a[iRow, jCol];
                    AV1[iRow * 2 + 1, jCol] = _a[iRow, jCol] + _v[iRow, jCol];
                }
            }

            for (var iRow = 0; iRow < _a.GetLength(0); iRow++)
            {
                for (var jCol = 0; jCol < _a.GetLength(1); jCol++)
                {
                    try
                    {
                        AU2[iRow * 2, jCol] = _a[iRow, jCol];
                        AU2[iRow * 2 + 1, jCol] = (_a[iRow, jCol] + _a[iRow + 1, jCol]) / 2.0 + _u[iRow, jCol];
                    }
                    catch { }
                }
            }

            for (var iRow = 0; iRow < _a.GetLength(0); iRow++)
            {
                for (var jCol = 0; jCol < _a.GetLength(1); jCol++)
                {
                    try
                    {
                        AV2[iRow*2, jCol] = _a[iRow, jCol];
                        AV2[iRow*2 + 1, jCol] = (_a[iRow, jCol] + _a[iRow + 1, jCol])/2.0 + _v[iRow, jCol];
                    }
                    catch
                    {
                    }
                }
            }

        }

        #endregion

    }
}
