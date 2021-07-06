namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Character Extensions 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class CharacterExtensions {
    #region Public

    /// <summary>
    /// Character (numeric value) to rational number
    /// </summary>
    public static BigRational ToRational(this char value) => new (value);

    #endregion Public
  }

}
