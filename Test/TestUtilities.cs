﻿using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Meta.Numerics;
using Meta.Numerics.Analysis;
using Meta.Numerics.Matrices;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace Test {

    internal static class TestUtilities {

        public static EvaluationSettings RelativeTarget = new EvaluationSettings() {
            RelativePrecision = 1.0E-13, AbsolutePrecision = 0.0
        };

        public static bool IsNearlyEqual (double x, double y, EvaluationSettings s) {
            if (Double.IsPositiveInfinity(x) && Double.IsPositiveInfinity(y)) return (true);
            if (Double.IsNegativeInfinity(x) && Double.IsNegativeInfinity(y)) return (true);
            if (Double.IsNaN(x) && Double.IsNaN(y)) return (true);
            double norm = Math.Abs(x) + Math.Abs(y);
            double tol = Math.Max(0.0, s.AbsolutePrecision) + norm * Math.Max(0.0, s.RelativePrecision);
            return (Math.Abs(x - y) <= tol);
        }

        public static bool IsNearlyEqual (Complex x, Complex y, EvaluationSettings s) {
            double m = ComplexMath.Abs(x) + ComplexMath.Abs(y);
            double e = s.AbsolutePrecision + m * s.RelativePrecision;
            return (ComplexMath.Abs(x - y) <= e);
        }

        public static bool IsNearlyEqual (AnyRectangularMatrix x, AnyRectangularMatrix y, EvaluationSettings s) {
            double m = x.FrobeniusNorm() + y.FrobeniusNorm();
            double e = Math.Max(0.0, s.AbsolutePrecision) + m * Math.Max(0.0, s.RelativePrecision);
            AnyRectangularMatrix D = x - y;
            return (D.FrobeniusNorm() <= e);
        }

        // Equality testing

        // allow last three digits to deviate
        public static readonly double TargetPrecision = Math.Pow(2.0,-42);

        // equality of reals

        public static bool IsNearlyEqual (double x, double y) {
            return (IsNearlyEqual(x, y, RelativeTarget));
            //return (IsNearlyEqual(x, y, TargetPrecision));
        }

        public static bool IsNearlyEqual (double x, double y, double e) {
            return (IsNearlyEqual(x, y, new EvaluationSettings() { RelativePrecision = e, AbsolutePrecision = e }));
        }

        // equality of complexes

        public static bool IsNearlyEqual (Complex u, Complex v) {
            return (IsNearlyEqual(u, v, RelativeTarget));
        }

        public static bool IsNearlyEqual (Complex u, Complex v, double e) {

            if (ComplexMath.Abs(u - v) <= e * (ComplexMath.Abs(u) + ComplexMath.Abs(v))) {
                return (true);
            } else {
                return (false);
            }
        }

        // equality of uncertain values

        public static bool IsNearlyEqual (UncertainValue u, UncertainValue v) {
            return (IsNearlyEqual(u.Value, v.Value) && IsNearlyEqual(u.Uncertainty, v.Uncertainty));
        }

        // equality of sums; this deals with "fair" loss of precision due to cancelation

        public static bool IsSumNearlyEqual (double x1, double x2, double y) {
            return (IsSumNearlyEqual(x1, x2, y, TargetPrecision));
        }


        public static bool IsSumNearlyEqual (double x1, double x2, double y, double e) {
            double x = x1 + x2;
            double u1 = Math.Abs(x1) * e;
            double u2 = Math.Abs(x2) * e;
            double u = u1 + u2;
            double v = Math.Abs(y) * e;

            //Console.WriteLine("  {0:g16} ?= {1:g16} ({2:g16})", x, y, u + v);
            if (Math.Abs(x - y) <=(u + v)) {
                return (true);
            } else {
                return (false);
            }
        }

        public static bool IsSumNearlyEqual (IEnumerable<double> xs, double y) {

            double sum = 0.0;
            double error = 0.0;
            foreach (double x in xs) {
                sum += x;
                error += TargetPrecision * Math.Abs(x);
            }
            error += TargetPrecision * Math.Abs(y);

            if (Math.Abs(sum - y) <= error) {
                return (true);
            } else {
                return (false);
            }
        }

        public static bool IsSumNearlyEqual(IEnumerable<double> xs, double y, EvaluationSettings settings) {

            double sum = 0.0;
            double norm = 0.0;
            foreach (double x in xs) {
                sum += x;
                norm += Math.Abs(x);
            }
            if (Double.IsNaN(sum) && Double.IsNaN(y)) return (true);
            double tol = settings.AbsolutePrecision + settings.RelativePrecision * norm;
            return (Math.Abs(sum - y) <= tol);

        }

        public static bool IsSumNearlyEqual (Complex z1, Complex z2, Complex zz) {
            Complex z = z1 + z2;

            double u1 = ComplexMath.Abs(z1) * TargetPrecision;
            double u2 = ComplexMath.Abs(z2) * TargetPrecision;
            double u = u1 + u2;
            double uu = ComplexMath.Abs(zz) * TargetPrecision;

            if (2.0 * ComplexMath.Abs(z - zz) <= (u + uu)) {
                return (true);
            } else {
                return (false);
            }
        }

        public static bool IsSumNearlyEqual (IEnumerable<Complex> zs, Complex zz) {

            Complex sum = 0.0;
            double error = 0.0;
            foreach (Complex z in zs) {
                sum += z;
                error += TargetPrecision * ComplexMath.Abs(z);
            }
            error += TargetPrecision * ComplexMath.Abs(zz);

            return (ComplexMath.Abs(sum - zz) <= error);

        }



        public static bool IsNearlyEqual (double[] u, double[] v) {
            return (IsNearlyEqual(u, v, TargetPrecision));
        }

        public static bool IsNearlyEqual (double[] u, double[] v, double e) {
            int d = u.Length;
            double norm = 0.0;
            for (int i = 0; i < d; i++) {
                norm += Math.Abs(u[i]) + Math.Abs(v[i]);
            }
            for (int i = 0; i < d; i++) {
                if (Math.Abs(u[i] - v[i]) > e * norm) return (false);
            }
            return (true);
        }


        // matrix creation utilities

        public static SquareMatrix CreateSquareUnitMatrix (int n) {
            SquareMatrix I = new SquareMatrix(n);
            for (int i = 0; i < n; i++) {
                I[i, i] = 1.0;
            }
            return (I);
        }

        public static SymmetricMatrix CreateSymmetricHilbertMatrix (int n) {
            SymmetricMatrix H = new SymmetricMatrix(n);
            H.Fill((r, c) => 1.0 / (r + c + 1));
            return (H);
        }

        public static SymmetricMatrix CreateSymmetricRandomMatrix (int n, int seed) {
            SymmetricMatrix M = new SymmetricMatrix(n);
            Random rng = new Random(seed);
            for (int r = 0; r < n; r++) {
                for (int c = 0; c <= r; c++) {
                    M[r, c] = 2.0 * rng.NextDouble() - 1.0;
                }
            }
            return (M);
        }

        public static bool IsNearlyEqual (AnyRectangularMatrix A, AnyRectangularMatrix B, double e) {
            double nA = A.FrobeniusNorm();
            double nB = B.FrobeniusNorm();
            double tol = e * (nA + nB);
            for (int r = 0; r < A.RowCount; r++) {
                for (int c = 0; c < A.ColumnCount; c++) {
                    if (Math.Abs(A[r, c] - B[r, c]) > tol) return (false);
                }
            }
            return (true);
        }

        public static bool IsNearlyEqual (AnyRectangularMatrix A, AnyRectangularMatrix B) {
            return (IsNearlyEqual(A, B, TargetPrecision));
        }

        public static bool IsNearlyEigenpair (AnySquareMatrix A, ColumnVector v, double a) {

            // compute products
            ColumnVector Av = A * v;
            ColumnVector av = a * v;

            // compute tolerance
            int d = v.Dimension;
            double N = A.MaxNorm();
            double n = v.MaxNorm();
            double ep = TargetPrecision * (Math.Abs(N*n)/d + Math.Abs(a*n));

            // compare elements within tolerance
            for (int i = 0; i < d; i++) {
                if (Math.Abs(Av[i] - av[i]) > ep) return (false);
            }
            return (true);

        }

        public static bool IsNearlyEigenpair (AnySquareMatrix A, ComplexColumnVector v, Complex a) {

            int d = A.Dimension;

            // compute products
            Complex[] Av = new Complex[d];
            for (int i=0; i<d; i++) {
                Av[i] = 0.0;
                for (int j=0; j<d; j++) {
                    Av[i] += A[i,j]*v[j];
                }
            }
            ComplexColumnVector av = a * v;

            // compute tolerance
            double N = A.MaxNorm();
            double n = 0.0;
            for (int i = 0; i < d; i++) {
                n += ComplexMath.Abs(v[i]);
            }
            double ep = TargetPrecision * (N * n / d + ComplexMath.Abs(a) * n);

            // compare elements within tolerance
            for (int i = 0; i < d; i++) {
                if (ComplexMath.Abs(Av[i] - av[i]) > ep) return (false);
            }
            return (true);

        }

        // Number generation

        public static double[] GenerateUniformRealValues (double a, double b, int n) {
            double[] result = new double[n];
            Random rng = new Random(1);
            for (int i = 0; i < n; i++) {
                result[i] = a + (b - a) * rng.NextDouble();
            }
            return (result);
        }

        // returns n positive reals numbers distributed log-uniformly between a and b

        public static double[] GenerateRealValues (double a, double b, int n) {
            return (GenerateRealValues(a, b, n, 1));
        }

        public static double[] GenerateRealValues (double a, double b, int n, int seed) {
            if ((a <= 0.0) || (b <= 0.0)) throw new ArgumentException();
            double la = Math.Log(a);
            double lb = Math.Log(b);
            double[] result = new double[n];
            Random rng = new Random(seed);
            for (int i = 0; i < n; i++) {
                result[i] = Math.Exp(la + (lb - la) * rng.NextDouble()); 
                //result[i] = Math.Pow(10.0, a + (b - a) * rng.NextDouble());
            }
            return (result);
        }

        // returns n complex distributed logarithmicly between 10^a and 10^b in all four quadrants
        public static Complex[] GenerateComplexValues (double a, double b, int n, Random rng) {
            if ((a <= 0.0) || (b <= 0.0)) throw new ArgumentException();
            double la = Math.Log(a);
            double lb = Math.Log(b);
            Complex[] result = new Complex[n];
            for (int i = 0; i < n; i++) {
                double re = Math.Exp(la + (lb - la) * rng.NextDouble());
                double im = Math.Exp(la + (lb - la) * rng.NextDouble());
                switch (rng.Next(4)) {
                    case 0:
                        break;
                    case 1:
                        im = -im;
                        break;
                    case 2:
                        re = -re;
                        im = -im;
                        break;
                    case 3:
                        re = -re;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                //if (rng.NextDouble() < 0.5) re = -re;
                //if (rng.NextDouble() < 0.5) im = -im;
                result[i] = new Complex(re, im);
            }
            return (result);
        }

        public static Complex[] GenerateComplexValues (double a, double b, int n) {
            return (GenerateComplexValues(a, b, n, new Random(1)));
        }

        // returns n positive integers distributed logarithmicly between a and b
        public static int[] GenerateIntegerValues (int a, int b, int n) {
            if ((a <= 0) || (b <= 0)) throw new ArgumentException();
            double la = Math.Log(a);
            double lb = Math.Log(b);
            int[] result = new int[n];
            Random rng = new Random(1);
            for (int i = 0; i < n; i++) {
                result[i] = (int) Math.Round(Math.Exp(la + (lb - la) * rng.NextDouble()));
                //result[i] = (int) Math.Round(Math.Pow(10.0, a + (b - a) * rng.NextDouble()));
            }
            return (result);
        }

        public static int[] GenerateUniformIntegerValues (int a, int b, int n) {
            int[] result = new int[n];
            Random rng = new Random(1);
            for (int i = 0; i < n; i++) {
                result[i] = rng.Next(a, b + 1);
            }
            return (result);
        }

        public static Sample CreateSample (ContinuousDistribution distribution, int count) {
            return (CreateSample(distribution, count, 1));
        }

        public static Sample CreateSample (ContinuousDistribution distribution, int count, int seed) {

            Sample sample = new Sample();

            Random rng = new Random(seed);
            for (int i = 0; i < count; i++) {
                double x = distribution.GetRandomValue(rng);
                sample.Add(x);
            }

            return (sample);
        }

        public static IEnumerable<double> CreateDataSample (Random rng, ContinuousDistribution distribution, int count) {
            return (distribution.GetRandomValues(rng, count));
        }

        public static void TestEnumerable<T>(IEnumerable<T> source) where T : IEquatable<T>
        {
            IEnumerator<T> typed = source.GetEnumerator();
            IEnumerator untyped = ((IEnumerable)source).GetEnumerator();

            for (int i = 0; i < 4; i++)
            {
                bool typedMove = typed.MoveNext();
                bool untypedMove = untyped.MoveNext();
                Assert.IsTrue(typedMove == untypedMove);
                if (!typedMove) break;
                T typedResult = typed.Current;
                T untypedResult = (T) untyped.Current;
                Assert.IsTrue(typedResult.Equals(untypedResult));
            }
        }

    }

}
