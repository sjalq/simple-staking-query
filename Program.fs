﻿open Nethereum.Web3
open SimpleStaking.Contracts.VaraToken
open SimpleStaking.Contracts.VaraToken.ContractDefinition
open SimpleStaking.Contracts.SimpleStaking
open SimpleStaking.Contracts.SimpleStaking.ContractDefinition
open TestBase
open System.Linq
open System.Numerics
open System.Threading
open System
open Nethereum.Web3.Accounts
open Nethereum.RPC.Eth.DTOs


type Actions =
    | Stake of BigInteger
    | Unstake


let pickRandomAction () = 
    let action = rnd.Next(0, 10) % 2
    match action with
    | 0 -> 
        let amount = 
            rnd.Next(0, 1_000_000) 
            |> BigInteger 
            |> (*) (BigInteger 1_000_000_000_000_000UL)
        Stake amount
    | 1 -> Unstake
    | _ -> failwith "should not happen"


let takeRandomActionAsRandomAccount varaTokenAddress simpleStakingAddress account =
    let varaTokenService = VaraTokenService(Web3(account, "http://localhost:8545"), varaTokenAddress)
    let simpleStakingService = SimpleStakingService(Web3(account, "http://localhost:8545"), simpleStakingAddress)

    let stake amount = 
        let approveTxr = 
            varaTokenService.ApproveRequestAndWaitForReceiptAsync(
                ApproveFunction(Spender = simpleStakingService.ContractHandler.ContractAddress, Amount = amount)
            ) |> runNow
        
        try 
            simpleStakingService.StakeRequestAndWaitForReceiptAsync(
                StakeFunction(Amount = amount)
            ) |> runNow 
            |> Ok
        with
        | ex -> Error ex

    let unstake =
        try 
            simpleStakingService.UnstakeRequestAndWaitForReceiptAsync(
                UnstakeFunction()
            ) |> runNow
            |> Ok
        with
        | ex -> Error ex

    // wait some seconds between actions (allows "days" of blocktime to pass)
    System.Threading.Thread.Sleep(rnd.Next(1,120))
    let action = pickRandomAction()
    let result = 
        match action with
        | Stake amount -> stake amount
        | Unstake -> unstake
    
    match result with
    | Ok txr -> 
        sprintf "Account: %s Action: %s Tx: %s" account.Address (action.ToString()) txr.TransactionHash
        |> Console.info
        |> ignore
    | Error ex ->
        sprintf "Account: %s Action: %s Error: %s" account.Address (action.ToString()) ex.Message
        |> ignore

let ABI = """[{"inputs":[{"internalType":"contract IERC20","name":"_vara","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"_staker","type":"address"},{"indexed":false,"internalType":"uint256","name":"_amount","type":"uint256"},{"indexed":false,"internalType":"uint256","name":"_duration","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_releaseDate","type":"uint256"}],"name":"Staked","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"_staker","type":"address"},{"indexed":false,"internalType":"uint256","name":"_amount","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_originalReleaseDate","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_releaseDate","type":"uint256"}],"name":"Unstaked","type":"event"},{"inputs":[{"internalType":"uint256","name":"_amount","type":"uint256"}],"name":"Stake","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"Unstake","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"releaseDates","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"stakedFunds","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"vara","outputs":[{"internalType":"contract IERC20","name":"","type":"address"}],"stateMutability":"view","type":"function"}]"""

let initRandomAccounts x = 
    sprintf "Spinning up %d random accounts" x |> Console.info
    let randomAccounts = 
        Enumerable.Range(1, x) 
        |> Seq.map (fun _ -> makeAccount()) 
        |> Seq.toArray

    "Giving all accounts some Eth" |> Console.info
    randomAccounts
    |> Array.map (fun acc -> 
        let ethTxr = ethConn.SendEther acc.Address (bigInt 1_000_000_000_000_000_000UL)
        sprintf "Account: %s Eth: 1" acc.Address 
        |> Console.info)
    |> ignore

    randomAccounts


let deployContracts () = 
    // deploy contracts
    Console.info "Deploying contracts"
    let varaTokenDeployment = VaraTokenDeployment()
    let varaTokenService = 
        VaraTokenService.DeployContractAndGetServiceAsync(ethConn.Web3, varaTokenDeployment) 
        |> Async.RunSynchronously 
    let simpleStakingDeployment = SimpleStakingDeployment(Vara = varaTokenService.ContractHandler.ContractAddress)
    let simpleStakingService = 
        SimpleStakingService.DeployContractAndGetServiceAsync(ethConn.Web3, simpleStakingDeployment) 
        |> Async.RunSynchronously
    (varaTokenService, simpleStakingService)


let giftVaraToAccounts (accounts: Account array) (varaTokenService:VaraTokenService) =
    Console.info "Initializing accounts with VARA"
    accounts
    |> Array.map (fun acc -> 
        let rndVaraBalance = 
            rnd.Next(0, 1_000_000) 
            |> BigInteger 
            |> (*) (BigInteger 1_000_000_000_000_000UL)
            
        let varaTxr = 
            varaTokenService
                .TransferRequestAndWaitForReceiptAsync(
                    TransferFunction(
                        To = acc.Address,
                        Amount = rndVaraBalance))
            |> runNow
        sprintf "Account: %s, VARA:\t %A" acc.Address rndVaraBalance |> Console.info
        varaTxr)


let advanceTime interval fn =
    ethConn.TimeTravel (1UL * days)
    let timer = new Timer(TimerCallback fn)
    timer.Change(0, interval) |> ignore
    timer


let mutable escapeKeyPressed = false
Thread(fun () -> 
    while not escapeKeyPressed do
        escapeKeyPressed <- Console.ReadKey(false).Key = ConsoleKey.Escape)
    .Start()


let useContractAtRandom = 
    let x = rnd.Next(1, 100)
    let randomAccounts = initRandomAccounts x
    let (varaTokenService, simpleStakingService) = deployContracts()
    giftVaraToAccounts randomAccounts varaTokenService |> ignore

    // travel forward in time 1 day every second, which allows unstaking to actually happen
    let timer = advanceTime 1000 (fun _ -> 
                                    // get the vara balance of the staking contract and display it
                                    let stakingContractBalance = 
                                        varaTokenService.BalanceOfQueryAsync(BalanceOfFunction(Account = simpleStakingService.ContractHandler.ContractAddress)) 
                                        |> runNow
                                    sprintf "Staking contract balance: %A" ((stakingContractBalance / (BigInteger 1_000_000_000_000_000UL))) |> Console.ok
                                )

    while not escapeKeyPressed do
        randomAccounts
        |> Array.Parallel.map (fun acc ->
            Thread.Sleep(rnd.Next(0, 30)) // wait between 0 and 30 seconds between actions
            acc
            |> takeRandomActionAsRandomAccount  
                varaTokenService.ContractHandler.ContractAddress 
                simpleStakingService.ContractHandler.ContractAddress)
        |> ignore

    timer.Dispose() 

    getEvents<StakedEventDTO> 
        "Staked"
        ABI
        simpleStakingService.ContractHandler.ContractAddress
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest()) 
    |> Seq.toList
    |> List.map JsonUtility.toJson
    |> Console.debug
    |> ignore

    getEvents<UnstakedEventDTO>
        "Unstaked"
        ABI
        simpleStakingService.ContractHandler.ContractAddress
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest())
    |> Seq.toList
    |> List.filter (fun e -> e.Amount > (BigInteger(0UL)))
    |> List.map JsonUtility.toJson
    |> Console.debug
    |> ignore