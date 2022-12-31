module Query

open System.Numerics


type Msg = 
    | AdvanceTimeTo of BigInteger
    | Stake of string * BigInteger
    | Unstake of string
    | MerkleDropReward of BigInteger


type Cmd = 
    | Nope
    | Payouts of Map<string, BigInteger>
    

type Model =
    {   TimeStamp : BigInteger
        StakedFunds : Map<string, BigInteger>
        ReleaseDates : Map<string, BigInteger>
        StakeShares : Map<string, BigInteger>
        TotalStakeShare : BigInteger }


let minStakingPeriod = 90UL * TestBase.days |> BigInteger


let update msg model = 
    let unstake account = 
        let originalReleaseDate = model.ReleaseDates |> Map.tryFind account
        match originalReleaseDate with
        | None -> model
        | Some originalReleaseDate ->
            let releaseDateDifference = BigInteger.Max(originalReleaseDate - model.TimeStamp, 0)
            let newReleaseDates = model.ReleaseDates.Remove(account)
            let stakeSharesToRemove = model.StakedFunds.[account] * releaseDateDifference
            let newStakedFunds = model.StakedFunds.Remove(account)
            let newTotalStakeShare = model.TotalStakeShare - stakeSharesToRemove
            let newStakeShares = model.StakeShares.Change(account, fun old -> old |> Option.map ((-) stakeSharesToRemove))
            { model with 
                StakedFunds = newStakedFunds
                TotalStakeShare = newTotalStakeShare
                StakeShares = newStakeShares
                ReleaseDates = newReleaseDates }

    match msg with
    | AdvanceTimeTo newTime ->
        { model with TimeStamp = newTime }, Cmd.Nope

    | Stake (account, amount) ->
        let model = 
            if model.StakedFunds.ContainsKey(account) then
                unstake account
            else
                model
        let stakeShares = amount * minStakingPeriod
        let newStakedFunds = model.StakedFunds.Add(account, stakeShares)
        let newReleaseDates = model.ReleaseDates.Add(account, model.TimeStamp + minStakingPeriod)
        let newTotalStakeShare = model.TotalStakeShare + stakeShares
        let newStakeShares = model.StakeShares.Change(account, fun old -> old |> Option.map ((+) stakeShares))
        { model with StakedFunds = newStakedFunds
                     TotalStakeShare = newTotalStakeShare
                     StakeShares = newStakeShares
                     ReleaseDates = newReleaseDates }, Cmd.Nope
    
    | Unstake account ->
        unstake account, Cmd.Nope

    | MerkleDropReward amount ->
        let applicableStakeShares = 
            model.StakeShares
            |> Map.map (fun account stakeShare -> 
                let releaseDateDiff = BigInteger.Max(model.ReleaseDates.[account] - model.TimeStamp, 0)
                stakeShare - (model.StakedFunds.[account] * releaseDateDiff))
        let payouts = 
            applicableStakeShares
            |> Map.map (fun _ stakeShare -> (stakeShare * amount) / model.TotalStakeShare)
        let newStakeShares = 
            model.StakeShares
            |> Map.map (fun account _ -> 
                let releaseDateDiff = BigInteger.Max(model.ReleaseDates.[account] - model.TimeStamp, 0)
                model.StakedFunds.[account] * releaseDateDiff)
        let newTotalStakeShare = newStakeShares |> Map.values |> Seq.sum
        { model with StakeShares = newStakeShares
                     TotalStakeShare = newTotalStakeShare }, Cmd.Payouts payouts 


let initModel timestamp = 
    { TimeStamp = timestamp
      StakedFunds = Map.empty
      ReleaseDates = Map.empty
      StakeShares = Map.empty
      TotalStakeShare = BigInteger.Zero }


let simulateModel model msgList = 
    msgList 
    |> List.fold 
        (fun (model, cmd) msg -> 
            sprintf "%A" cmd |> Console.info
            let (model, cmd) = update msg model
            match cmd with
            | Payouts payouts -> payouts |> Map.iter (fun account amount -> Console.ok (sprintf "%s: %A" account amount))
            | Nope -> ()
            model, cmd) 
        (model, Cmd.Nope)

