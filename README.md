# Discrete Math library 

This **.Net** routine uses `System.Numerics.BigInteger`

## Examples:

```c#
using HigherArithmetics;
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

var ratio = new BigRational(3, 4);

// 11 / 4
Console.WriteLine($"{ratio + 2}");

// 9
Console.WriteLine(Modulo.ModDivision(511, 137, 19));

```
