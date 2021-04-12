// namespace GeomancerServer {
//   public struct FixedK {
//     private const int PRECISION = 1000;
//     
//     private readonly long x;
//
//     private FixedK(long x) {
//       this.x = x;
//     }
//
//     public static FixedK FromInt(long n) {
//       return new FixedK(n * PRECISION);
//     }
//     public long ToInt() {
//       return x / PRECISION;
//     }
//     public static FixedK operator+(FixedK a, FixedK b) {
//       return new FixedK(a.x + b.x);
//     }
//     public static FixedK operator-(FixedK a, FixedK b) {
//       return new FixedK(a.x - b.x);
//     }
//     public static FixedK operator*(FixedK a, FixedK b) {
//       return new FixedK(a.x * b.x / PRECISION);
//     }
//     public static FixedK operator/(FixedK a, FixedK b) {
//       return new FixedK(a.x * PRECISION / b.x);
//     }
//   }
// }