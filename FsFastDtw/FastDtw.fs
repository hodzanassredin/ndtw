namespace FastDtw

open Internal

/// FastDtw algorithm for calculating series distance
type FastDtw () =

  /// Calculate a FastDtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radiusG
  /// Returns: distance between series
  static member Distance (series1: double[][]) (series2: double[][]) (radius: int) distance :double =
    let (cost, _) = fastDtwWithPath series1 series2 radius distance
    cost

  /// Calculate a FastDtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radius
  /// Returns: (distance between series, correlated point maps)
  static member DistanceWithPath (series1: double[][]) (series2: double[][]) (radius: int) distance :(double * Point list) =
    fastDtwWithPath series1 series2 radius distance
