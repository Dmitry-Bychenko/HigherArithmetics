# Discrete Math library 

[![NuGet version (HigherArithmetics)](https://img.shields.io/nuget/v/HigherArithmetics.svg?style=flat-square)](https://www.nuget.org/packages/HigherArithmetics/) 

This **.Net** routine uses `System.Numerics.BigInteger`

## Examples:

```c#
using HigherArithmetics;
using HigherArithmetics.Linq;
using HigherArithmetics.Numerics;
using HigherArithmetics.Primes;

using System.Numerics;
...

// Cached prime numbers
KnownPrimes.MaxKnownPrime = 1500;

// 80
Console.WriteLine(DiscreteMath.Totient(123)); 

// 1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30, 60
Console.WriteLine(string.Join(", ", Divisors.AllDivisors(60))); 

var ratio = new BigRational(1, 4) + BigRational.Parse("11/37") + BigRational.Parse("1.2(41)");;

// 131041 / 73260
Console.WriteLine($"Answer is {ratio}");

// 9
Console.WriteLine(Modulo.ModDivision(511, 137, 19));

// AAA, AAB, AAC, ABB, ABC, BBC
Console.WriteLine(string.Join(", ", "ABCABA".Combinations(3).Select(item => string.Concat(item))));
```
