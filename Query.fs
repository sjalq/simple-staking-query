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


let clamp min max value = 
    if value < min then min
    elif value > max then max
    else value


let upsert key value map =
    match map |> Map.tryFind key with
    | None -> map |> Map.add key value
    | Some _ -> map |> Map.remove key |> Map.add key value


let addValueToMap key amount map =
    match map |> Map.tryFind key with
    | None -> 
        map 
        |> Map.add key amount
    | Some value -> upsert key (value + amount) map


let unstake account model = 
    let originalReleaseDate = model.ReleaseDates |> Map.tryFind account
    let newModel = 
        match originalReleaseDate with
        | None -> model
        | Some originalReleaseDate ->
            let releaseDateDifference = clamp BigInteger.Zero minStakingPeriod (originalReleaseDate - model.TimeStamp)  
            let newReleaseDates = model.ReleaseDates |> Map.remove account
            let stakeSharesToRemove = BigInteger (-1) * model.StakedFunds.[account] * releaseDateDifference 
            if stakeSharesToRemove > model.StakeShares.[account] then 
                stakeSharesToRemove |> sprintf "Cannot unstake more than staked (%A)" |> failwith
            let newStakedFunds = model.StakedFunds.Remove(account)
            let newTotalStakeShare = model.TotalStakeShare + stakeSharesToRemove
            if newTotalStakeShare < BigInteger.Zero then 
                    newTotalStakeShare |> sprintf "TotalStakeShare cannot be negative (%A)" |> failwith
            let newStakeShares = model.StakeShares |> addValueToMap account stakeSharesToRemove
            { model with 
                StakedFunds = newStakedFunds
                TotalStakeShare = newTotalStakeShare
                StakeShares = newStakeShares
                ReleaseDates = newReleaseDates }
    newModel, Cmd.Nope


let stake account amount model =
        let stakeShare = amount * minStakingPeriod
        let newStakedFunds = model.StakedFunds |> addValueToMap account amount 
        let newReleaseDates = model.ReleaseDates |> upsert account (model.TimeStamp + minStakingPeriod) 
        let newTotalStakeShare = model.TotalStakeShare + stakeShare
        let newStakeShares = addValueToMap account stakeShare model.StakeShares
        { model with 
            StakedFunds = newStakedFunds
            TotalStakeShare = newTotalStakeShare
            StakeShares = newStakeShares
            ReleaseDates = newReleaseDates }, Cmd.Nope

let merkleDropReward _ amount model =
    let applicableStakeShares = 
        model.StakeShares
        |> Map.map (fun account stakeShare -> 
            let releaseDateDiff = 
                match model.ReleaseDates |> Map.tryFind account with
                | None -> BigInteger.Zero
                | Some releaseDate ->
                    max (releaseDate - model.TimeStamp) BigInteger.Zero
            let stakedFunds = model.StakedFunds |> Map.tryFind account |> Option.defaultValue BigInteger.Zero
            stakeShare - (stakedFunds * releaseDateDiff))

    let payouts = 
        applicableStakeShares
        |> Map.map (fun _ stakeShare ->
            if stakeShare * amount = BigInteger.Zero then
                BigInteger.Zero
            elif model.TotalStakeShare = BigInteger.Zero then
                 failwith "TotalStakeShare cannot be zero"
            else
                (stakeShare * amount) / model.TotalStakeShare)

    let newStakeShares = 
        model.StakeShares
        |> Map.map (fun account _ -> 
            let releaseDateDiff = 
                match model.ReleaseDates |> Map.tryFind account with
                | None -> BigInteger.Zero
                | Some releaseDate ->
                    max (releaseDate - model.TimeStamp) BigInteger.Zero
            let stakedFunds = model.StakedFunds |> Map.tryFind account |> Option.defaultValue BigInteger.Zero
            stakedFunds * releaseDateDiff)

    let newTotalStakeShare = newStakeShares |> Map.values |> Seq.sum

    { model with 
        StakeShares = newStakeShares
        TotalStakeShare = newTotalStakeShare }, Cmd.Payouts payouts 


let rec update msg model = 
    
    let updatetime timestamp = 
        if timestamp <> model.TimeStamp then 
            update (AdvanceTimeTo timestamp) model |> fst
        else 
            model

    
    match msg with
    | AdvanceTimeTo newTime ->
        sprintf "Advancing Time To: %A" newTime |> Console.info
        { model with TimeStamp = newTime }, Cmd.Nope

    | Call (timestamp, account, stakingMsg) ->
        
        let model = updatetime timestamp    

        //sprintf "\nBefore" |> Console.info
        //model |> Console.dbg |> ignore
        sprintf "Message: %A" msg |> Console.info
        //sprintf "After" |> Console.info

        match stakingMsg with
        | Stake amount ->
            stake account amount model
        
        | Unstake  ->
            unstake account model
    
        | MerkleDropReward amount ->
            merkleDropReward account amount model
        // |> Console.dbg 


let initModel timestamp = 
    { TimeStamp = timestamp
      StakedFunds = Map.empty
      ReleaseDates = Map.empty
      StakeShares = Map.empty
      TotalStakeShare = BigInteger.Zero }
