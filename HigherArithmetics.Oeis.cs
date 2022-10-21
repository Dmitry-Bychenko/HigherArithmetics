using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// On-line Encyclopedia Integer Sequence (OEIS)
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class Oeis : IComparable<Oeis>, IEquatable<Oeis>, IReadOnlyList<BigInteger> {
    #region Private Data

    private static readonly HttpClient s_Http;

    private static readonly CookieContainer s_CookieContainer;

    #endregion Private Data

    #region Algorithm

    private static async IAsyncEnumerable<Oeis> CoreQuery(string query, [EnumeratorCancellation] CancellationToken token) {
      int pageSize = 10;
      int count = -1;

      for (int page = 0; count < 0 || page * pageSize < count; ++page) {
        string address = $"https://oeis.org/search?fmt=json&q={query}&start={page * pageSize}";

        using var req = new HttpRequestMessage {
          Method = HttpMethod.Get,
          RequestUri = new Uri(address),
          Headers = {
          { HttpRequestHeader.Accept.ToString(), "text/json" },
        },
          Content = new StringContent("", Encoding.UTF8, "text/json")
        };

        using Stream stream = await s_Http.GetStreamAsync(address, token).ConfigureAwait(false);

        using JsonDocument doc = await JsonDocument.ParseAsync(stream, default, token);

        if (count < 0)
          count = doc.RootElement.GetProperty("count").GetInt32();

        var arrayCount = doc.RootElement.GetProperty("results").GetArrayLength();

        for (int i = 0; i < arrayCount; ++i) {
          var item = doc.RootElement.GetProperty("results")[i];

          yield return new Oeis(item);
        }
      }

      yield break;
    }

    #endregion Algorithm

    #region Create

    static Oeis() {
      try {
        ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls |
          SecurityProtocolType.Tls11 |
          SecurityProtocolType.Tls12;
      }
      catch (NotSupportedException) {
        ;
      }

      s_CookieContainer = new CookieContainer();

      var handler = new HttpClientHandler() {
        CookieContainer = s_CookieContainer,
        Credentials = CredentialCache.DefaultCredentials,
      };

      s_Http = new HttpClient(handler);
    }

    private Oeis() {
      Id = 0;
      Title = "Not existing";
      Items = Array.Empty<BigInteger>();
    }

    private Oeis(JsonElement json) {
      Id = json.GetProperty("number").GetInt32();
      Title = json.GetProperty("name").GetString() ?? "";
      Items = json
         .GetProperty("data")
         .GetString()
        ?.Split(',')
        ?.Select(item => BigInteger.Parse(item))
        ?.ToList() ?? new List<BigInteger>();
    }

    /// <summary>
    /// Non-existing sequence
    /// </summary>
    public static Oeis None { get; } = new Oeis();

    /// <summary>
    /// Enumerate sequences given by their items 
    /// </summary>
    /// <param name="source">Items</param>
    /// <param name="token">Cancellation tokens</param>
    /// <returns>OEIS sequencies</returns>
    /// <exception cref="ArgumentNullException">When source is null</exception>
    public static async IAsyncEnumerable<Oeis> Find(IEnumerable<BigInteger> source, [EnumeratorCancellation] CancellationToken token = default) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      string query = string.Join(",", source);

      await foreach (Oeis item in CoreQuery(query, token))
        yield return item;
    }

    /// <summary>
    /// Find by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="token">Cancellation token if exists</param>
    /// <returns>Sequence found</returns>
    public static async Task<Oeis> Find(int id, CancellationToken token = default) {
      if (id <= 0 || id >= 10_000_000)
        return None;

      string query = $"A{id:d6}";

      await foreach (Oeis item in CoreQuery(query, token))
        return item;

      return None;
    }

    /// <summary>
    /// Find by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="token">Cancellation token if exists</param>
    /// <returns>Sequence found</returns>
    public static async Task<Oeis> Find(string id, CancellationToken token = default) {
      if (string.IsNullOrWhiteSpace(id))
        return None;

      id = id.Trim().TrimStart('a', 'A');

      return int.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out int intId)
        ? await Find(intId, token)
        : None;
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Compare
    /// </summary>
    public static int Compare(Oeis left, Oeis right) {
      if (ReferenceEquals(left, right))
        return 0;
      if (left is null)
        return -1;
      if (right is null)
        return +1;

      return left.Id.CompareTo(right.Id);
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Items
    /// </summary>
    public IReadOnlyList<BigInteger> Items { get; }

    /// <summary>
    /// Link to the article
    /// </summary>
    private string Link => $"https://oeis.org/A{Id:d6}";

    /// <summary>
    /// Report
    /// </summary>
    /// <returns></returns>
    public string ToReport() {
      string text = string.Join(Environment.NewLine + "       ",
        Title.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

      return string.Join(Environment.NewLine,
        $"Id     : A{Id:d6}",
        $"Link   : {Link}",
        $"Name   : {text}",
        $"Values : {string.Join(", ", Items)}"
      );
    }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => $"A{Id:d6}";

    #endregion Public

    #region IComparable<Oeis>

    /// <summary>
    /// Compare To
    /// </summary>
    public int CompareTo([AllowNull] Oeis other) => Compare(this, other);

    #endregion IComparable<Oeis>

    #region IEquatable<Oeis>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(Oeis other) => other is not null && other.Id == Id;

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => obj is Oeis other && Equals(other);

    /// <summary>
    /// Hash code
    /// </summary>
    public override int GetHashCode() => Id;

    #endregion IEquatable<Oeis>

    #region IReadOnlyList<BigInteger>

    /// <summary>
    /// Number of first items read
    /// </summary>
    public int Count => Items.Count;

    /// <summary>
    /// Get first index item
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>Top index item</returns>
    /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
    public BigInteger this[int index] => index >= 0 && index < Count
      ? Items[index]
      : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Typed Enumerator
    /// </summary>
    public IEnumerator<BigInteger> GetEnumerator() => Items.GetEnumerator();

    /// <summary>
    /// Typeless Enumerator
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    #endregion IReadOnlyList<BigInteger>
  }

}
