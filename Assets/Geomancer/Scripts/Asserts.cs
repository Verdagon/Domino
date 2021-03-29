using System;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

// namespace Geomancer.Scripts {
  public static class Asserts {
    public static void Assert(
        bool condition,
        string message = "",
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) {
      if (!condition) {
        throw new AssertionException("Error at " + sourceFilePath + ":" + sourceLineNumber + " " + memberName + ": ", message);
      }
    }
  }
// }
