// open FsCheck
// open FsCheck.Xunit

// let modelGen =
//   Gen.map4
//     (fun timeStamp stakedFunds releaseDates stakeShares totalStakeShare ->
//       { TimeStamp = timeStamp; StakedFunds = stakedFunds; ReleaseDates = releaseDates; StakeShares = stakeShares; TotalStakeShare = totalStakeShare })
//     (Arb.generate<BigInteger>)
//     (Map.ofSeq (Arb.generate<seq<string * BigInteger>>))
//     (Map.ofSeq (Arb.generate<seq<string * BigInteger>>))
//     (Map.ofSeq (Arb.generate<seq<string * BigInteger>>))
//     (Arb.generate<BigInteger>)

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should always return a valid model`` (model: Model, account: string) =
//   let model', _ = Query.unstake account model
//   let totalStakeShare = 
//     model'.StakeShares 
//     |> Map.values
//     |> Seq.sum
//   let isModelValid = 
//     model'.TotalStakeShare = totalStakeShare
//   isModelValid

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should always return a Cmd.Nope`` (model: Model, account: string) =
//   let _, cmd = Query.unstake account model
//   cmd = Query.Cmd.Nope

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should decrease the total stake share`` (model: Model, account: string) =
//   let originalTotalStakeShare = model.TotalStakeShare
//   let model', _ = Query.unstake account model
//   let newTotalStakeShare = model'.TotalStakeShare
//   newTotalStakeShare < originalTotalStakeShare

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should decrease the stake share for the given account`` (model: Model, account: string) =
//   let originalStakeShare = model.StakeShares |> Map.tryFind account |> Option.defaultValue BigInteger.Zero
//   let model', _ = Query.unstake account model
//   let newStakeShare = model'.StakeShares |> Map.tryFind account |> Option.defaultValue BigInteger.Zero
//   newStakeShare < originalStakeShare

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should remove the account from the release dates map`` (model: Model, account: string) =
//   let model', _ = Query.unstake account model
//   not (model'.ReleaseDates |> Map.containsKey account)

// [<Property(QuietOnSuccess = true)>]
// let ``unstake should remove the account from the staked funds map`` (model: Model, account: string) =
//   let model', _ = Query.unstake account model
//   not (model'.StakedFunds |> Map.containsKey account)