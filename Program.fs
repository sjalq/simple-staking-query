open Nethereum.Web3
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
open Query
open System.Threading.Tasks

type Actions =
    | Stake of BigInteger
    | Unstake


type DividendEvent = 
    { 
        Timestamp : BigInteger
        Amount : BigInteger
    }


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

    let unstake () =
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
        | Unstake -> unstake ()
    
    match result with
    | Ok txr -> 
        sprintf "Account: %s Action: %s Tx: %s" account.Address (action.ToString()) txr.TransactionHash
        //|> Console.info
        |> ignore
    | Error ex ->
        sprintf "Account: %s Action: %s Error: %s" account.Address (action.ToString()) ex.Message
        |> Console.info
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
let monitorEscapeKey () =
    Console.info "Press ESC to stop"
    Thread(fun () -> 
        while not escapeKeyPressed do
            escapeKeyPressed <- Console.ReadKey(false).Key = ConsoleKey.Escape)
        .Start()

let runFunctionInThread fn =
    let t = Thread(fun () -> 
        while not escapeKeyPressed do 
            Thread.Sleep(rnd.Next(0, 120000)) 
            fn()
    )
    t

let useContractAtRandom () = 
    let randomAccounts = initRandomAccounts 10
    let (varaTokenService, simpleStakingService) = deployContracts()
    giftVaraToAccounts randomAccounts varaTokenService |> ignore

    // print the deployed contract addresses to the screen
    sprintf "VARA Address : %A" varaTokenService.ContractHandler.ContractAddress |> Console.complete 
    sprintf "Staking Address : %A" simpleStakingService.ContractHandler.ContractAddress |> Console.complete

    // travel forward in time 1 day every second, which allows unstaking to actually happen
    let timer = advanceTime 1000 (fun _ -> 
                                    // get the vara balance of the staking contract and display it
                                    let stakingContractBalance = 
                                        varaTokenService.BalanceOfQueryAsync(BalanceOfFunction(Account = simpleStakingService.ContractHandler.ContractAddress)) 
                                        |> runNow
                                    let time = ethConn.getLatestBlockTimestamp()
                                    sprintf "Time: %A - \t Staking contract balance: %A" time ((stakingContractBalance / (BigInteger 1_000_000_000_000_000UL))) |> Console.ok
                                )

    monitorEscapeKey()
    let threads = 
        randomAccounts
        |> Array.map (fun acc ->
            runFunctionInThread (fun () -> 
                acc
                |> takeRandomActionAsRandomAccount  
                    varaTokenService.ContractHandler.ContractAddress 
                    simpleStakingService.ContractHandler.ContractAddress))
    
    threads |> Array.iter (fun t -> t.Start())
    
    while threads |> Array.exists(fun t -> t.IsAlive) do
        // wait for all the threads to conclude
        Thread.Sleep(1000)

    timer.Dispose() 

    let plug = ContractPlug(ethConn, abiFromAbiJson(ABI), simpleStakingService.ContractHandler.ContractAddress)

    plug.getEvents<StakedEventDTO> 
        "Staked"
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest()) 
    |> Seq.toList
    |> ignore

    plug.getEvents<UnstakedEventDTO>
        "Unstaked"
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest())
    |> Seq.toList
    |> List.filter (fun e -> e.Amount > (BigInteger(0UL)))
    |> ignore


let queryFromEvents 
        timestamp 
        (stakingEvents:StakedEventDTO list) 
        (unstakingEvents:UnstakedEventDTO list) 
        dividendEvents =
    let initModel = Query.initModel timestamp
    let stakingMsg = 
        stakingEvents
        |> List.map (fun se -> (se.ReleaseDate - se.Duration), Query.Msg.Stake (se.Staker, se.Amount))
        |> List.sortBy fst
    let unstakingMsg = 
        unstakingEvents
        |> List.map (fun ue -> ue.ReleaseDate, Query.Msg.Unstake ue.Staker)
        |> List.sortBy fst
    let dividendMsg =
        dividendEvents
        |> List.map (fun de -> de.Timestamp, Query.Msg.MerkleDropReward de.Amount)
        |> List.sortBy fst
    let allMsg = 
        stakingMsg @ unstakingMsg @ dividendMsg 
        |> List.sortBy fst 
        |> Console.debug
        |> List.map snd
    allMsg
    |> List.fold 
        (fun ((model,cmd), payments) msg -> 
            match cmd with
            | Query.Cmd.Payouts p -> 
                Query.update msg model, payments @ [p]
            | _ -> 
                Query.update msg model, payments)
        ((initModel, Query.Cmd.Nope), List.empty)


let fetchEventsAndSimulateDividends
        nodeUri
        stakingContractAddress 
        contractLaunchTimestamp
        dividendEvents =
    let conn = EthereumConnection(nodeUri, makeAccount().PrivateKey) 
    let plug = ContractPlug(conn, abiFromAbiJson(ABI), stakingContractAddress)
    let stakingEvents = 
        plug.getEvents<StakedEventDTO> 
            "Staked"
            (BlockParameter(0UL))
            (BlockParameter.CreateLatest()) 
        |> Seq.toList
    let unstakingEvents =
        plug.getEvents<UnstakedEventDTO>
            "Unstaked"
            (BlockParameter(0UL))
            (BlockParameter.CreateLatest())
        |> Seq.toList
    queryFromEvents 
        contractLaunchTimestamp 
        stakingEvents 
        unstakingEvents
        dividendEvents

let usage =
    @"Invalid arguments:
    Use either no arguments to run a simulation against ""http://127.0.0.1:8545""
    Or provide [nodeUri] [contractAddress] [dividendTimestamp1] [dividendAmount1] [dividendTimestamp2] [dividendAmount2] ..."


[<EntryPoint>]
let main argv =
    match argv |> Array.toList with
    | nodeUri :: contractAddress :: strDividends when (List.length strDividends) % 2 = 0 && (List.length strDividends) <> 0 -> 
        let dividendEvents = 
            strDividends
            |> List.chunkBySize 2
            |> List.mapi (fun indx strDividend -> 
                match strDividend with
                | [strTimestamp; strAmount] -> 
                    try 
                        { Timestamp = UInt64.Parse strTimestamp |> BigInteger
                          Amount = Decimal.Multiply(Decimal.Parse strAmount, 1_000_000_000_000_000_000M) |> BigInteger }
                        |> Ok
                    with
                    | _ -> sprintf "Invalid timestamp or amount for dividend %d" (indx+1) |> Error
                | _ -> sprintf "Invalid dividend event %d" (indx+1) |> Error)

        let errors = dividendEvents |> List.choose (fun i -> match i with | Error x -> Some x | _ -> None)
        match errors with
        | [] ->
            let dividendEvents = dividendEvents |> List.choose (function | Ok x -> Some x | _ -> None)
            let (model, payments) =
                fetchEventsAndSimulateDividends nodeUri contractAddress BigInteger.Zero dividendEvents
            payments |> List.iter (fun p -> sprintf "%A" p |> Console.debug |> ignore)
        | _ ->
            Console.error usage
            Console.error "\nErrors:"
            errors |> List.iter Console.error
    | [] -> useContractAtRandom()
    | _ -> 
        Console.error usage
            
    0