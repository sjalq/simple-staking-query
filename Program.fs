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
open Nethereum.RPC.Eth.DTOs
open FileFormats
open Nethereum.RPC.TransactionManagers
open Nethereum.Model


type Actions =
    | Stake of BigInteger
    | Unstake


type DividendEvent = 
    { 
        Timestamp : BigInteger
        Amount : BigInteger
    }


let varaABI = """[{"inputs":[],"stateMutability":"nonpayable","type":"constructor"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"owner","type":"address"},{"indexed":true,"internalType":"address","name":"spender","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Approval","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"from","type":"address"},{"indexed":true,"internalType":"address","name":"to","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Transfer","type":"event"},{"inputs":[{"internalType":"address","name":"owner","type":"address"},{"internalType":"address","name":"spender","type":"address"}],"name":"allowance","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"approve","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"decimals","outputs":[{"internalType":"uint8","name":"","type":"uint8"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"subtractedValue","type":"uint256"}],"name":"decreaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"addedValue","type":"uint256"}],"name":"increaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"name","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"symbol","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"totalSupply","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"transfer","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"from","type":"address"},{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"transferFrom","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]"""
let stakingABI = """[{"inputs":[{"internalType":"contract IERC20","name":"_vara","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"_staker","type":"address"},{"indexed":false,"internalType":"uint256","name":"_amount","type":"uint256"},{"indexed":false,"internalType":"uint256","name":"_duration","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_releaseDate","type":"uint256"}],"name":"Staked","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"_staker","type":"address"},{"indexed":false,"internalType":"uint256","name":"_amount","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_originalReleaseDate","type":"uint256"},{"indexed":true,"internalType":"uint256","name":"_releaseDate","type":"uint256"}],"name":"Unstaked","type":"event"},{"inputs":[{"internalType":"uint256","name":"_amount","type":"uint256"}],"name":"Stake","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"Unstake","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"releaseDates","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"stakedFunds","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"vara","outputs":[{"internalType":"contract IERC20","name":"","type":"address"}],"stateMutability":"view","type":"function"}]"""


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
    let ethc = EthereumConnection(localURI, account)
    let varaTokenPlug = ContractPlug(ethc, abiFromAbiJson(varaABI), varaTokenAddress)
    let simpleStakingPlug = ContractPlug(ethc, abiFromAbiJson(stakingABI), simpleStakingAddress)

    let stake amount = 
        let approveTxr = varaTokenPlug.ExecuteFunction "approve" [|simpleStakingAddress; amount |]
        let varaBalance = varaTokenPlug.Query<BigInteger> "balanceOf" [|account.Address|] 
        try 
            simpleStakingPlug.ExecuteFunction "Stake" [| amount |] |> Ok
        with
        | ex -> Error ex

    let unstake () =
        try 
            "Attemping to unstake" |> Console.info
            simpleStakingPlug.ExecuteFunction "Unstake" [||] |> Ok
        with
        | ex -> Error ex

    let action = pickRandomAction()
    let result =    
        match action with
        | Stake amount -> stake amount
        | Unstake -> unstake ()
    
    match result with
    | Ok txr -> 
        sprintf "Account: %s Action: %s Tx: %s" account.Address (action.ToString()) txr.TransactionHash
        |> Console.info
        |> ignore
    | Error ex ->
        sprintf "Account: %s Action: %s Error: %s" account.Address (action.ToString()) ex.Message
        |> Console.info
        |> ignore


let initRandomAccounts x = 
    sprintf "Spinning up %d random accounts" x |> Console.info
    let randomAccounts = 
        Enumerable.Range(1, x) 
        |> Seq.map (fun _ -> makeAccount()) 
        |> Seq.toArray
    randomAccounts


let giftEther (accounts:AccountWrapper array) =
    "Giving all accounts some Eth" |> Console.info
    accounts
    |> Array.map (fun acc -> 
        let ethTxr = ethConn.SendEther acc.Address (bigInt 1_000_000_000_000_000_000UL)
        sprintf "Account: %s Eth: 1" acc.Address 
        |> Console.info)
    |> ignore


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


let giftVaraToAccounts (accounts: AccountWrapper array) (varaTokenService:VaraTokenService) =
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


let mutable escapeKeyPressed = false
let monitorEscapeKey () =
    Console.info "Press ESC to stop"
    Thread(fun () -> 
        while not escapeKeyPressed do
            escapeKeyPressed <- Console.ReadKey(false).Key = ConsoleKey.Escape)
        .Start()


let simulatedDay = 100 // one day will pass every 100ms


let runFunctionInThread fn =
    let t = Thread(fun () -> 
        while not escapeKeyPressed do 
            Thread.Sleep(rnd.Next(0, 30 * simulatedDay)) 
            fn()
    )
    t.Start()
    t


let advanceTime interval fn =
    let timer = new Timer(TimerCallback fn)
    timer.Change(0, interval) |> ignore
    timer


let useContractAtRandom (addressToImpersonate:string) = 
    let randomAccounts = 
        initRandomAccounts 10 
        |> Array.map (fun acc -> Actual acc) 
        |> Array.append [|Impersonated addressToImpersonate|]
    giftEther randomAccounts
    let (varaTokenService, simpleStakingService) = deployContracts()
    giftVaraToAccounts randomAccounts varaTokenService |> ignore

    // print the deployed contract addresses to the screen
    sprintf "VARA Address : %A" varaTokenService.ContractHandler.ContractAddress |> Console.complete 
    sprintf "Staking Address : %A" simpleStakingService.ContractHandler.ContractAddress |> Console.complete

    // travel forward in time 1 day every second, which allows unstaking to actually happen
    let timer = advanceTime simulatedDay (fun _ -> 
                                    ethConn.TimeTravel (1UL * days)
                                    // get the vara balance of the staking contract and display it
                                    let stakingContractBalance = 
                                        varaTokenService.BalanceOfQueryAsync(BalanceOfFunction(Account = simpleStakingService.ContractHandler.ContractAddress)) 
                                        |> runNow
                                    let time = ethConn.getLatestBlockTimestamp()
                                    sprintf "Time: %A - \t Staking contract balance: %A" time ((stakingContractBalance / (BigInteger 1_000_000_000_000_000UL))) |> Console.ok
                                    sprintf "%A" simpleStakingService.ContractHandler.ContractAddress |> Console.complete
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
        
    while threads |> Array.exists(fun t -> t.IsAlive) do
        // wait for all the threads to conclude
        Thread.Sleep(1000)

    timer.Dispose() 

    let plug = ContractPlug(ethConn, abiFromAbiJson(stakingABI), simpleStakingService.ContractHandler.ContractAddress)

    plug.getEvents<StakedEventDTO> 
        "Staked"
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest()) 
    |> Seq.toList
    |> Seq.where (fun e -> (e.Staker.ToLowerInvariant()) = (addressToImpersonate.ToLowerInvariant()))
    |> Seq.map (fun e -> {| Staker = e.Staker |})
    |> Console.dbg
    |> ignore

    plug.getEvents<UnstakedEventDTO>
        "Unstaked"
        (BlockParameter(0UL))
        (BlockParameter.CreateLatest())
    |> Seq.toList
    |> List.filter (fun e -> e.Amount > (BigInteger(0UL)))
    |> Seq.where (fun e -> e.Staker = addressToImpersonate)
    |> Seq.map (fun e -> {| Staker = e.Staker |})
    |> Console.dbg
    |> ignore


let getTimestamp msg = 
    match msg with
    | Query.Msg.Call (timestamp, _, Query.StakingMsg.Stake _) -> (BigInteger.One + timestamp)
    | Query.Msg.Call (timestamp, _, _) -> timestamp
    | Query.Msg.AdvanceTimeTo timestamp -> timestamp
    

let queryFromEvents 
        timestamp 
        (stakingEvents:StakedEventDTO list) 
        (unstakingEvents:UnstakedEventDTO list) 
        dividendEvents =
    let initModel = Query.initModel timestamp
    let stakingMsgs = 
        stakingEvents
        |> List.map (fun se -> 
            Query.Msg.Call ((se.ReleaseDate - se.Duration), se.Staker, Query.StakingMsg.Stake se.Amount))
        |> List.sortBy getTimestamp
    let unstakingMsgs = 
        unstakingEvents
        |> List.map (fun ue ->
            Query.Msg.Call (ue.ReleaseDate, ue.Staker, Query.StakingMsg.Unstake) )
        |> List.sortBy getTimestamp
    let dividendMsgs =
        dividendEvents
        |> List.map (fun de -> 
            Query.Msg.Call (de.Timestamp, "0xSantaClaus", Query.StakingMsg.MerkleDropReward de.Amount))
        |> List.sortBy getTimestamp
    let jointEvents = 
        stakingMsgs @ unstakingMsgs @ dividendMsgs 
        |> List.sortBy getTimestamp 
    let allMsg = 
        jointEvents 

    allMsg
    |> List.fold 
        (fun (model,cmd) msg -> Query.update msg model)
        (initModel, Query.Cmd.Nope)

let fetchEventsAndSimulateDividends
        nodeUri
        stakingContractAddress 
        contractLaunchTimestamp
        dividendEvents =
    let conn = EthereumConnection(nodeUri, Actual (makeAccount())) 
    let plug = ContractPlug(conn, abiFromAbiJson(stakingABI), stakingContractAddress)
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
    Use either [addressToImpersonate] to run a simulation against ""http://127.0.0.1:8545"" 
    Or provide [nodeUri] [contractAddress] [dividendTimestamp1] [dividendAmount1] [dividendTimestamp2] [dividendAmount2] ..."


[<EntryPoint>]
let main argv =
    //system("resize -s 150 180 > /dev/null") |> ignore

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
            let (model, payouts) =
                fetchEventsAndSimulateDividends nodeUri contractAddress BigInteger.Zero dividendEvents

            payouts |> sprintf "%A" |> Console.ok |> ignore
            match payouts with
            | Query.Cmd.Payouts payouts -> payouts |> toFile
            | _ -> ()
        | _ ->
            Console.error usage
            Console.error "\nErrors:"
            errors |> List.iter Console.error

    | [addressToImpersonate] -> addressToImpersonate.ToLowerInvariant() |> useContractAtRandom
    | _ -> 
        Console.error usage
            
    0