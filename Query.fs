module Query

open System.Numerics

type StakingMsg = 
    | Stake of BigInteger
    | Unstake
    | MerkleDropReward of BigInteger

type Msg = 
    | AdvanceTimeTo of BigInteger
    | Call of BigInteger * string * StakingMsg

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


let unstake account model = 
    let originalReleaseDate = model.ReleaseDates |> Map.tryFind account
    let newModel = 
        match originalReleaseDate with
        | None -> model
        | Some originalReleaseDate ->
            "Unstaking" |> Console.dbg |> ignore
            let releaseDateDifference = 
                max (originalReleaseDate - model.TimeStamp) BigInteger.Zero 
                |> min minStakingPeriod
            sprintf "StakeShares %A" (model.StakeShares |> Map.tryFind account) |> Console.dbg |> ignore 
            let newReleaseDates = model.ReleaseDates.Remove(account)
            sprintf "StakedFunds %A" (model.StakedFunds |> Map.tryFind account) |> Console.dbg |> ignore 
            let stakeSharesToRemove = model.StakedFunds.[account] * releaseDateDifference |> Console.dbg
            sprintf "StakeShares %A" model.StakeShares.[account] |> Console.dbg |> ignore
            if stakeSharesToRemove > model.StakeShares.[account] then 
                stakeSharesToRemove |> sprintf "Cannot unstake more than staked (%A)" |> failwith
            let newStakedFunds = model.StakedFunds.Remove(account)
            let newTotalStakeShare = model.TotalStakeShare - stakeSharesToRemove
            if newTotalStakeShare < BigInteger.Zero then 
                    newTotalStakeShare |> sprintf "TotalStakeShare cannot be negative (%A)" |> failwith
            let newStakeShare = 
                model.StakeShares 
                |> Map.tryFind account 
                |> Option.defaultValue BigInteger.Zero
                |> (-) stakeSharesToRemove
            let newStakeShares = 
                model.StakeShares 
                |> Map.remove account 
                |> Map.add account newStakeShare
            { model with 
                StakedFunds = newStakedFunds
                TotalStakeShare = newTotalStakeShare
                StakeShares = newStakeShares
                ReleaseDates = newReleaseDates }
    newModel, Cmd.Nope


let stake account amount model =
        let stakeShare = amount * minStakingPeriod
        let newStakedFunds = model.StakedFunds.Add(account, amount) 
        let newReleaseDates = model.ReleaseDates.Add(account, model.TimeStamp + minStakingPeriod)
        let newTotalStakeShare = model.TotalStakeShare + stakeShare
        let newStakeShare = 
            model.StakeShares 
            |> Map.tryFind account 
            |> Option.defaultValue BigInteger.Zero
            |> (+) stakeShare
        let newStakeShares = 
            model.StakeShares 
            |> Map.remove account 
            |> Map.add account newStakeShare
        { model with 
            StakedFunds = newStakedFunds
            TotalStakeShare = newTotalStakeShare
            StakeShares = newStakeShares
            ReleaseDates = newReleaseDates }, Cmd.Nope

let merkleDropReward account amount model =
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
    { model with 
        StakeShares = newStakeShares
        TotalStakeShare = newTotalStakeShare }, Cmd.Payouts payouts 


let rec update msg model = 
    sprintf "Message: %A" msg |> Console.info
    sprintf "Model: %A" model |> Console.complete

    let updatetime timestamp = 
        if timestamp <> model.TimeStamp then 
            update (AdvanceTimeTo timestamp) model |> fst
        else 
            model

    match msg with
    | AdvanceTimeTo newTime ->
        { model with TimeStamp = newTime }, Cmd.Nope

    | Call (timestamp, account, stakingMsg) ->
        let model = updatetime timestamp    

        match stakingMsg with
        | Stake amount ->
            stake account amount model
        
        | Unstake  ->
            unstake account model
    
        | MerkleDropReward amount ->
            merkleDropReward account amount model
            
    |> Console.debug


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
            sprintf "simmi %A" cmd |> Console.error
            let (model, cmd) = update msg model
            match cmd with
            | Payouts payouts -> payouts |> Map.iter (fun account amount -> Console.ok (sprintf "%s: %A" account amount))
            | Nope -> ()
            model, cmd) 
        (model, Cmd.Nope)

