﻿using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Meta.Numerics.Matrices;
using Meta.Numerics.Functions;

namespace Test {

    
    [TestClass()]
    public class SymmetricMatrixTest {


        private TestContext testContextInstance;

        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        private static SymmetricMatrix CreateSymmetricRandomMatrix (int n, int seed) {
            SymmetricMatrix M = new SymmetricMatrix(n);
            Random rng = new Random(seed);
            for (int r = 0; r < n; r++) {
                for (int c = 0; c <= r; c++) {
                    M[r, c] = 2.0 * rng.NextDouble() - 1.0;
                }
            }
            return (M);
        }

        [TestMethod]
        public void SymmetricMatrixAccess () {

            SymmetricMatrix M = CreateSymmetricRandomMatrix(4, 1);

            // check column
            ColumnVector c = M.Column(1);
            Assert.IsTrue(c.Dimension == M.Dimension);
            for (int i = 0; i < c.Dimension; i++) {
                Assert.IsTrue(c[i] == M[i, 1]);
            }
            
            // check row
            RowVector r = M.Row(1);
            Assert.IsTrue(r.Dimension == M.Dimension);
            for (int i = 0; i < r.Dimension; i++) {
                Assert.IsTrue(r[i] == c[i]);
            }            
            
            // check clone
            SymmetricMatrix MC = M.Copy();
            Assert.IsTrue(MC == M);
            Assert.IsFalse(MC != M);

            // check independence of clone
            MC[0, 1] += 1.0;
            Assert.IsFalse(MC == M);
            Assert.IsTrue(MC != M);

            // check that update was symmetric
            Assert.IsTrue(M[0, 1] == M[1, 0]);

        }

        [TestMethod]
        public void SymmetricMatrixArithmetic () {
            SymmetricMatrix M = CreateSymmetricRandomMatrix(5,1);

            // addition is same a multiplication by two
            SymmetricMatrix MA = M + M;
            SymmetricMatrix M2 = 2.0 * M;
            Assert.IsTrue(MA == M2);

            // division by two should return us to the original
            // SymmetricMatrix MB = M2 / 2.0;

            // subtraction of self same as multiplication by zero
            SymmetricMatrix MS = M - M;
            SymmetricMatrix M0 = 0.0 * M;
            Assert.IsTrue(MS == M0);

            // negation is same as multiplication by negative one
            SymmetricMatrix MN = -M;
            SymmetricMatrix MM = -1.0 * M;
            Assert.IsTrue(MM == MN);

        }

        [TestMethod]
        public void SymmetricHilbertMatrixInverse () {
            for (int d = 1; d < 4; d++) {
                SymmetricMatrix H = TestUtilities.CreateSymmetricHilbertMatrix(d);
                SymmetricMatrix HI = H.Inverse();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(HI * H, UnitMatrix.OfDimension(d)));
            }
            // fails for d >= 4; look into this
        }

        [TestMethod]
        public void SymmetricRandomMatrixInverse () {
            for (int d = 1; d <= 100; d = d + 11) {
                SymmetricMatrix M = TestUtilities.CreateSymmetricRandomMatrix(d, 1);
                SymmetricMatrix MI = M.Inverse();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(MI * M, UnitMatrix.OfDimension(d)));
            }

        }

        [TestMethod]
        public void SymmetricRandomMatrixEigenvectors () {
            for (int d = 1; d <= 100; d = d + 11) {

                SymmetricMatrix M = CreateSymmetricRandomMatrix(d, 1);

                RealEigendecomposition E = M.Eigendecomposition();

                Assert.IsTrue(E.Dimension == M.Dimension);

                double[] eigenvalues = new double[E.Dimension];
                for (int i = 0; i < E.Dimension; i++) {
                    double e = E.Eigenpairs[i].Eigenvalue;
                    ColumnVector v = E.Eigenpairs[i].Eigenvector;
                    // The eigenvector works
                    Assert.IsTrue(TestUtilities.IsNearlyEigenpair(M, v, e));
                    // The eigenvalue is the corresponding diagonal value of D
                    Assert.IsTrue(E.DiagonalizedMatrix[i, i] == e);
                    // Remember eigenvalue to take sum in future
                    eigenvalues[i] = e;
                }

                // The eigenvectors sum to trace
                double tr = M.Trace();
                Assert.IsTrue(TestUtilities.IsSumNearlyEqual(eigenvalues, tr));

                // The decomposition works
                Assert.IsTrue(TestUtilities.IsNearlyEqual(
                    E.DiagonalizedMatrix, E.TransformMatrix.Transpose * M * E.TransformMatrix
                ));

                // Transform matrix is orthogonal
                Assert.IsTrue(TestUtilities.IsNearlyEqual(
                    E.TransformMatrix.Transpose * E.TransformMatrix, UnitMatrix.OfDimension(d)
                ));
            }
        }

        [TestMethod]
        public void SymmetricHilbertMatrixEigenvalues () {
            for (int d = 1; d <= 8; d++) {
                SymmetricMatrix H = TestUtilities.CreateSymmetricHilbertMatrix(d);
                double tr = H.Trace();
                double[] es = H.Eigenvalues();
                Assert.IsTrue(TestUtilities.IsSumNearlyEqual(es, tr));
            }
        }

        [TestMethod]
        public void RealEigenvalueOrdering () {

            int d = 10;

            Random rng = new Random(d + 1);
            SymmetricMatrix A = new SymmetricMatrix(d);
            A.Fill((int r, int c) => -1.0 + 2.0 * rng.NextDouble());
            RealEigendecomposition E = A.Eigendecomposition();
            RealEigenpairCollection pairs = E.Eigenpairs;

            pairs.Sort(OrderBy.ValueAscending);
            for (int i = 1; i < pairs.Count; i++) {
                Assert.IsTrue(pairs[i - 1].Eigenvalue <= pairs[i].Eigenvalue);
                Assert.IsTrue(TestUtilities.IsNearlyEigenpair(A, pairs[i].Eigenvector, pairs[i].Eigenvalue));
             }

            pairs.Sort(OrderBy.ValueDescending);
            for (int i = 1; i < pairs.Count; i++) {
                Assert.IsTrue(pairs[i - 1].Eigenvalue >= pairs[i].Eigenvalue);
                Assert.IsTrue(TestUtilities.IsNearlyEigenpair(A, pairs[i].Eigenvector, pairs[i].Eigenvalue));
            }

            pairs.Sort(OrderBy.MagnitudeAscending);
            for (int i = 1; i < pairs.Count; i++) {
                Assert.IsTrue(Math.Abs(pairs[i - 1].Eigenvalue) <= Math.Abs(pairs[i].Eigenvalue));
                Assert.IsTrue(TestUtilities.IsNearlyEigenpair(A, pairs[i].Eigenvector, pairs[i].Eigenvalue));
            }

            pairs.Sort(OrderBy.MagnitudeDescending);
            for (int i = 1; i < pairs.Count; i++) {
                Assert.IsTrue(Math.Abs(pairs[i - 1].Eigenvalue) >= Math.Abs(pairs[i].Eigenvalue));
                Assert.IsTrue(TestUtilities.IsNearlyEigenpair(A, pairs[i].Eigenvector, pairs[i].Eigenvalue));
            }

        }

        [TestMethod]
        public void HilbertMatrixCholeskyDecomposition () {
            for (int d = 1; d <= 4; d++) {
                SymmetricMatrix H = TestUtilities.CreateSymmetricHilbertMatrix(d);

                // Decomposition succeeds
                CholeskyDecomposition CD = H.CholeskyDecomposition();
                Assert.IsTrue(CD != null);
                Assert.IsTrue(CD.Dimension == d);

                // Decomposition works
                SquareMatrix S = CD.SquareRootMatrix();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(S * S.Transpose, H));

                // Inverse works
                SymmetricMatrix HI = CD.Inverse();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(H * HI, UnitMatrix.OfDimension(d)));
            }
        }

        [TestMethod]
        public void SymmetricRandomMatrixCholeskyDecomposition () {

            int d = 100;
            Random rng = new Random(d);
            ColumnVector[] V = new ColumnVector[d];
            for (int i=0; i < d; i++) {
                V[i] = new ColumnVector(d);
                for (int j = 0; j < d; j++) {
                    V[i][j] = rng.NextDouble();
                }
            }

            SymmetricMatrix A = new SymmetricMatrix(d);
            for (int i = 0; i < d; i++) {
                for (int j = 0; j <= i; j++) {
                    A[i, j] = V[i].Transpose * V[j];
                }
            }

            Stopwatch s = Stopwatch.StartNew();
            CholeskyDecomposition CD = A.CholeskyDecomposition();
            s.Stop();
            Console.WriteLine("{0} {1}", d, s.ElapsedMilliseconds);

            Assert.IsTrue(CD != null);

        }

        [TestMethod]
        public void CatalanHankelMatrixDeterminant () {

            for (int d = 1; d <= 8; d++) {

                SymmetricMatrix S = new SymmetricMatrix(d);
                for (int r = 0; r < d; r++) {
                    for (int c = 0; c <= r; c++) {
                        int n = r + c;
                        S[r, c] = AdvancedIntegerMath.BinomialCoefficient(2*n, n) / (n + 1);
                    }
                }

                CholeskyDecomposition CD = S.CholeskyDecomposition();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(CD.Determinant(), 1.0));

            }

        }

        [TestMethod]
        public void SymmetricMatrixNorms () {

            SymmetricMatrix Z = new SymmetricMatrix(3);
            Assert.IsTrue(Z.OneNorm() == 0.0);
            Assert.IsTrue(Z.InfinityNorm() == 0.0);
            Assert.IsTrue(Z.FrobeniusNorm() == 0.0);

            SymmetricMatrix A = CreateSymmetricRandomMatrix(4, 1);
            Assert.IsTrue(A.OneNorm() > 0.0);
            Assert.IsTrue(A.InfinityNorm() > 0.0);
            Assert.IsTrue(A.FrobeniusNorm() > 0.0);

            SymmetricMatrix B = CreateSymmetricRandomMatrix(4, 2);
            Assert.IsTrue(B.OneNorm() > 0.0);
            Assert.IsTrue(B.InfinityNorm() > 0.0);
            Assert.IsTrue(B.FrobeniusNorm() > 0.0);

            SymmetricMatrix S = A + B;
            Assert.IsTrue(S.OneNorm() <= A.OneNorm() + B.OneNorm());
            Assert.IsTrue(S.InfinityNorm() <= A.InfinityNorm() + B.InfinityNorm());
            Assert.IsTrue(S.FrobeniusNorm() <= A.FrobeniusNorm() + B.FrobeniusNorm());

            SquareMatrix P = A * B;
            Assert.IsTrue(P.FrobeniusNorm() <= A.FrobeniusNorm() * B.FrobeniusNorm());
            // Frobenium norm is sub-multiplicative

        }


        [TestMethod]
        public void CholeskySolveExample () {

            // This is a very simple 3 X 3 problem I just wrote down to test the Cholesky solver
            SymmetricMatrix S = new SymmetricMatrix(3);
            S[0, 0] = 4.0;
            S[1, 0] = -2.0; S[1, 1] = 5.0;
            S[2, 0] = 1.0;  S[2, 1] = 3.0; S[2, 2] = 6.0;
            CholeskyDecomposition CD = S.CholeskyDecomposition();

            ColumnVector b = new ColumnVector(9.0, 7.0, 15.0);
            ColumnVector x = CD.Solve(b);
            Assert.IsTrue(TestUtilities.IsNearlyEqual(S * x, b));

        }

    }
}









