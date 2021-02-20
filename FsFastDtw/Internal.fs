namespace FastDtw

open System
open Microsoft.FSharp.Core.LanguagePrimitives

/// Internal functions for FastDtw algorithm.
/// These are not intended for external consumption
module Internal =

  /// Debug mode
  [<Literal>]
  let Debug = false 

  ///////////////////
  // Supporting types

  /// Indexing window for the cost grid
  /// (series1 min, series1 max, series2 min, series2 max)
  type Window = int * int * int * int

  /// Point in the cost grid. It represents the series1 and series2 indexes within the comparison.
  type Point = int * int

  ////////////
  // Functions

  /// Calculate distance between 2 values
  //let inline distance (a:double[]) (b:double[]) : double = (new DenseVector(a) - new DenseVector(b)).L2Norm()
  
  /// Determine maximum of two values
  let inline max a b = if a > b then a else b 

  /// Calculate mean of 2 numbers
  let inline mean (a:double[]) (b:double[]) = Array.init (a.Length) (fun i -> DivideByInt (a.[i] + b.[i]) 2) 

  /// Determine minimum of two values
  let inline min a b = if a < b then a else b 

  /// Determine min of three values
  let inline min3 a b c =
    if a <= b && a <= c then a
    else if b <= a && b <= c then b
    else c

  /// Fill a string with a specified character
  let charFill i c = 
    seq [1..i] 
    |> Seq.map (fun _ -> c) 
    |> String.concat ""

  /// Dump the cost grid to the console with an overlayed path
  /// This is primarily for debugging
  let dumpPathMatrix (series1: double[][]) (series2: double[][]) (costGrid: double[,]) (path: Point list) = 
    // Header (top)
    series2 |> Array.map(fun x -> sprintf "%s  " (Array.map (fun y -> sprintf "%5.1f  " y) x |> String.Concat)) |> String.Concat |> printfn "       %s"
    printfn "  PATH%s" (charFill (series2.Length * 7) "-")

    // Cost grid
    let mutable pathI: int = 0
    for i in 1..series1.Length do
      // Header (side)
      let strs = Array.map (sprintf "%5.1f| ") series1.[i-1]
      printf "%s| " (String.Join(",", strs))

      // data
      for j in 1..series2.Length do
        if i = fst path.[pathI] && j = snd path.[pathI]
        then
          pathI <- min (path.Length - 1) (pathI + 1)
          printf "%5.1f* " (costGrid.[i,j])
        else 
          printf "%5.1f  " (costGrid.[i,j])
      printfn ""

  /// Are coordinates within the defined window box 
  let inWindow (i: int) (j: int) (w: Window) =
    let (a, b, c , d) = w
    i >= a && i <= b && j >= c && j <= d
  
  /// Are coordinates within the defined window box 
  let inWindowOption (i: int) (j: int) (w: Window option) =
    match w with
    | Some w' -> inWindow i j w'
    | None -> false

  /// Are coordinates within the defined window boxes
  let inWindows (i: int) (j: int) (ws: Window option * Window option) =
    inWindowOption i j (fst ws) || inWindowOption i j (snd ws)

  // Determine the next best step, when the path must jump to the next window 
  let getCoordinatesFromNextWindow (i: int) (j: int) windowTail (costGrid: double[,]) =
    match windowTail with
    | h::_ ->
              // Determine shortest next step 
              let opt1 = if inWindow (i-1) j h then costGrid.[i-1,j] else infinity
              let opt2 = if inWindow i (j-1) h then costGrid.[i,j-1] else infinity
              let opt3 = if inWindow (i-1) (j-1) h then costGrid.[i-1,j-1] else infinity

              if opt1 < opt2 && opt1 < opt3 then (false, i-1, j)
              else if opt2 < opt1 && opt2 < opt3 then (false, i, j-1)
              else if i-1 = 0 && j-1 = 0 then (true, i, j)
              else (false, i-1, j-1)
    | []   -> (true, i, j)

  // TODO: backtracking path sometimes gets a higher value on the next step (maybe a window thing)
  /// Get the best cost path from the cost grid
  let getTravelledPath n m window costGrid =
    // printfn "gtp: %d %d %A %A" n m window costGrid
    // Walk backwards through the path
    let rec getTravelledPath' a i j windowHeads windowTail (costGrid:double[,]) =
      // printfn "gtp': a:%A i:%A j:%A wh:%A wt:%A" a i j windowHeads windowTail 
      if not (inWindowOption i j (fst windowHeads))
      then
        // Not in current window, remove and move to next window set
        match windowTail with
        | h::t -> getTravelledPath' a i j (snd windowHeads, Some h) t costGrid
        | []   -> if (snd windowHeads) = None 
                  then a 
                  else getTravelledPath' a i j (snd windowHeads, None) [] costGrid
      else
        // Determine shortest next step (based on the currently valid windows)
        let opt1 = if inWindows (i-1) j windowHeads then costGrid.[i-1,j] else infinity
        let opt2 = if inWindows i (j-1) windowHeads then costGrid.[i,j-1] else infinity
        let opt3 = if inWindows (i-1) (j-1) windowHeads then  costGrid.[i-1,j-1] else infinity

        if opt1 < opt2 && opt1 < opt3 then getTravelledPath' ((i,j) :: a) (i - 1) j windowHeads windowTail costGrid 
        else if opt2 < opt1 && opt2 < opt3 then getTravelledPath' ((i,j) :: a) i (j-1) windowHeads windowTail costGrid
        else getTravelledPath' ((i,j) :: a) (i-1) (j-1) windowHeads windowTail costGrid 

    match List.rev window with
    | h1::h2::t -> getTravelledPath' [] n m (Some h1, Some h2) t costGrid 
    | h::t      -> getTravelledPath' [] n m (Some h, None) t costGrid 
    | _         -> [] 


  /// Calculate dtw cost & travelled path
  let calculateDtw (a: double[][]) (b: double[][]) window (distance :Func<double[],double[],double>) :(double * Point list)  =
    let rec calculateDtw' i j n m (costGrid :double[,]) (a :double[][]) (b :double[][]) window :(double * Point list) =
      if i > n then
        let travelledPath = getTravelledPath n m window costGrid 
        (costGrid.[n,m], travelledPath)
      else if j > m then
        calculateDtw' (i + 1) 1 n m costGrid a b window
      else
        let cost = distance.Invoke(a.[i-1],b.[j-1])
        costGrid.[i,j] <- cost + (min3
                                    costGrid.[i-1,j]
                                    costGrid.[i,j-1]
                                    costGrid.[i-1,j-1])

        calculateDtw' (i) (j+1) n m costGrid a b window

    // Init path array
    let n = Array.length a
    let m = Array.length b
    let path = Array2D.init (n+1) (m+1) (fun _ _ -> 0.)
    [1..n] |> List.iter (fun x -> path.[x,0] <- Double.PositiveInfinity)
    [1..m] |> List.iter (fun x -> path.[0,x] <- Double.PositiveInfinity)

    calculateDtw' 1 1 n m path a b window

  /// Calculate Dtw cost and path, limit using windowing
  let calculateDtwWithWindow (series1: double[][]) (series2: double[][]) (window: Window list) (distance :Func<double[],double[],double>) :(double * Point list)  =
    let fullWindow = window
    let rec calculateDtwWithWindow' pw w i j nmin mmin n m (costGrid:double[,]) (a:double[][]) (b:double[][]) :(double * Point list) =
      if i > n then
        match w with
        | h::t -> // Done with this window, grab the next one 
                  let (nmin,nmax,mmin,mmax) = h
                  calculateDtwWithWindow' h t nmin mmin nmin mmin nmax mmax costGrid a b
        | []   -> // Out of windows, must be at the end
                  let w' = fullWindow
                  let travelledPath = getTravelledPath n m w' costGrid
                  if Debug then dumpPathMatrix a b costGrid travelledPath
                  (costGrid.[n,m], travelledPath)
      else if j > m then
        calculateDtwWithWindow' pw w (i + 1) mmin nmin mmin n m costGrid a b
      else
        let cost = distance.Invoke (a.[i-1],b.[j-1])
        let additionalCost = 
          match (i > nmin, j > mmin) with
          | (true, true)   -> min3
                                costGrid.[i-1,j]
                                costGrid.[i,j-1]
                                costGrid.[i-1,j-1]
          | (false, true)  -> costGrid.[i,j-1]
          | (true, false)  -> costGrid.[i-1,j]
          | (false, false) -> if i = 1 && j = 1 
                              then 0.
                              else 
                                let (pwminn, pwmaxn, pwminm, pwmaxm) = pw
                                if i >= pwminn && i <= pwmaxn && j >= pwminm && j <= pwmaxm 
                                then costGrid.[i,j] 
                                else infinity // TODO: can't get here, better than magic number?

        // printfn "i: %2d j: %2d c: %0.1f ac: %0.1f = %0.1f" i j cost additionalCost (cost + additionalCost)
        if costGrid.[i,j] = 0. || costGrid.[i,j] > cost + additionalCost then costGrid.[i,j] <- cost + additionalCost
        // path.[i,j] <- cost + (min3
        //     path.[i-1,j]
        //     path.[i,j-1]
        //     path.[i-1,j-1])

        calculateDtwWithWindow' pw w (i) (j+1) nmin mmin n m costGrid a b

    // Init path array
    let n = Array.length series1
    let m = Array.length series2
    let path = Array2D.init (n+1) (m+1) (fun _ _ -> 0.)
    [1..n] |> List.iter (fun x -> path.[x,0] <- Double.PositiveInfinity)
    [1..m] |> List.iter (fun x -> path.[0,x] <- Double.PositiveInfinity)

    let ((nmin,nmax,mmin,mmax), window') =
      match window with
      | h::t -> (h, t)
      | [] -> ((1,1,n,m), [])

    // printfn ">> %d %d %d %d" nmin mmin nmax mmax 
    calculateDtwWithWindow' (0,0,0,0) window' 1 1 nmin mmin nmax mmax path series1 series2

  /// Make an array of values more coarse (cut by half)
  let coarser (series :double[][]) :double[] [] =
    [|0 .. 2 .. series.Length - 1|]
    |> Array.map (fun i -> if i = series.Length - 1 
                           then series.[i] // Last item when odd number of items is special case
                           else mean series.[i] series.[i+1])

  /// Calculate dtw
  /// series1: First series
  /// series2: Second series
  /// windows: List of windows that the path must stay within
  /// Returns: distance between series and the point mapping of travelled path
  let dtw (series1: double[][]) (series2: double[][]) (windows: Window list option) distance :(double * Point list)  =
    match windows with
    | Some(w) -> calculateDtwWithWindow series1 series2 w distance
    | None    -> calculateDtwWithWindow series1 series2 [1, series1.Length, 1, series2.Length] distance

  /// Given a path, define the next-level-up valid windows for pathing
  let expandedResolutionWindow (path: Point list) (n: int) (m: int) (radius: int) :(Window list) = 
    // Build a radius around the provided path
    // Note: Must upRes the path, since it was halved, the upRes doubles the path coordinates as blocks
    path
    |> List.map (fun (i, j) ->
      let i' = i * 2 - 1
      let j' = j * 2 - 1 
      let imin = max (i' - radius) 1
      let imax = min (i' + radius + 1) n
      let jmin = max (j' - radius) 1
      let jmax = min (j' + radius + 1) m
      (imin, imax, jmin, jmax))

  /// Calculate a FastDtw distance and path between the provided series
  /// series1: First series
  /// series2: Second series
  /// radius: Search radius
  /// Returns: distance between series and the point mapping of travelled path
  let rec fastDtwWithPath (series1: double[][]) (series2: double[][]) (radius: int) distance :(double * Point list) =
    let minTSize= radius + 2

    if series1.Length < minTSize || series2.Length < minTSize
    then
      dtw series1 series2 None distance
    else
      let shrunkX = coarser series1
      let shrunkY = coarser series2

      let (cost, lowResPath) = fastDtwWithPath shrunkX shrunkY radius distance
      let window = expandedResolutionWindow lowResPath series1.Length series2.Length radius
      if Debug then printfn "cost: %A lowResPath: %A window: %A" cost lowResPath window

      dtw series1 series2 (Some window) distance
