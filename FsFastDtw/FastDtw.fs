namespace FastDtw

open Internal

/// FastDtw algorithm for calculating series distance
type FastDtw () =

  /// Calculate a FastDtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radiusG
  /// Returns: distance between series
  static member Distance (series1: float[][]) (series2: float[][]) (radius: int) :float =
    let (cost, _) = fastDtwWithPath series1 series2 radius
    cost

  /// Calculate a FastDtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radius
  /// Returns: (distance between series, correlated point maps)
  static member DistanceWithPath (series1: float[][]) (series2: float[][]) (radius: int) :(float * Point list) =
    fastDtwWithPath series1 series2 radius
