namespace FastDtw

open Internal

/// Dtw algorithm for calculating series distance
type Dtw () =

  /// Calculate a Dtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// Returns: distance between series
  static member Distance (series1: float[][]) (series2: float[][]) :float =
    let (cost, _) = dtw series1 series2 None
    cost

  /// Calculate a Dtw distance between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radius
  /// Returns: (distance between series, correlated point maps)
  static member DistanceWithPath (series1: float[][]) (series2: float[][]) (radius: int) :(float * Point list) =
    dtw series1 series2 None 