open Nethereum.Web3
open SimpleStaking.Contracts.VaraToken
open SimpleStaking.Contracts.VaraToken.ContractDefinition
open SimpleStaking.Contracts.SimpleStaking
open SimpleStaking.Contracts.SimpleStaking.ContractDefinition
open TestBase
open System.Linq
open System.Numerics

type Actions =
    | Stake of BigInteger
    | Unstake

let pickRandomAction () = 
    let action = rnd.Next(0, 10) % 2
    match action with
    | 0 -> 
        let amount = 
            rnd.Next(0, 1_000) 
            |> BigInteger 
            |> (*) (BigInteger 1_000_000_000_000_000_000UL)
        Stake amount
    | 1 -> Unstake
    | _ -> failwith "should not happen"


let takeRandomActionAsRandomAccount accounts varaTokenAddress simpleStakingAddress =
    let account = pickRandom accounts
    let varaTokenService = VaraTokenService(Web3(account, "http://localhost:8545"), varaTokenAddress)
    let simpleStakingService = SimpleStakingService(Web3(account, "http://localhost:8545"), simpleStakingAddress)

    let stake amount = 
        let approveTxr = 
            varaTokenService.ApproveRequestAndWaitForReceiptAsync(
                ApproveFunction(Spender = simpleStakingService.ContractHandler.ContractAddress, Amount = amount)
            ) |> runNow
        let stakeTxr = 
            simpleStakingService.StakeRequestAndWaitForReceiptAsync(
                StakeFunction(Amount = amount)
            ) |> runNow
        ()

    let unstake =
        let unstakeTxr = 
            simpleStakingService.UnstakeRequestAndWaitForReceiptAsync(
                UnstakeFunction()
            ) |> runNow
        ()

    match pickRandomAction() with
    | Stake amount -> stake amount
    | Unstake -> unstake


let useContractAtRandom = 
    // logic:
    // * deploy the VARA token
    // * deploy the SimpleStaking contract, pointing at the VARA token
    // * spin up x number of random accounts with some Eth and some VARA
    // * give all the accounts some Eth and some VARA
    // * loop 
    //      * randomly pick an account
    //      * give it some Eth
    //      * give it some VARA
    //      * randomly decide to stake or to unstake
    //          * if staking, randomly pick an amount to stake
    //          * if unstaking, simply unstake
    //      * wait for the transaction to complete
    //      * display the balances of all the account and the staking contract
    //      * repeat

    // deploy contracts
    let varaTokenDeployment = VaraTokenDeployment()
    let varaTokenService = 
        VaraTokenService.DeployContractAndGetServiceAsync(ethConn.Web3, varaTokenDeployment) 
        |> Async.RunSynchronously 
    let simpleStakingDeployment = SimpleStakingDeployment()
    simpleStakingDeployment.Vara <- varaTokenService.ContractHandler.ContractAddress
    let simpleStakingService = 
        SimpleStakingService.DeployContractAndGetServiceAsync(ethConn.Web3, simpleStakingDeployment) 
        |> Async.RunSynchronously

    // spin up x number of random accounts with some Eth and some VARA
    let x = rnd.Next(1, 100)
    let randomAccounts = 
        Enumerable.Range(1, x) 
        |> Seq.map (fun _ -> makeAccount()) 
        |> Seq.toList

    // give all the accounts some Eth and some VARA
    Console.info "Initializing accounts with Eth and VARA"
    let initTxrs = 
        randomAccounts
        |> List.map (fun acc -> 
            let ethTxr = ethConn.SendEther acc.Address (bigInt 1000000000000000000UL)

            let varaTxr = 
                varaTokenService
                    .TransferRequestAndWaitForReceiptAsync(
                        TransferFunction(
                            To = acc.Address,
                            Amount = bigInt 300_000_000_000_000_000UL))
                |> runNow
                
            ethTxr, varaTxr)
    
    let balances = 
        randomAccounts
        |> List.map (fun acc -> 
            let ethBalance = ethConn.Web3.Eth.GetBalance.SendRequestAsync(acc.Address) |> runNow
            let varaBalanceOfFunction = BalanceOfFunction(Account = acc.Address)
            let varaBalance = varaTokenService.BalanceOfQueryAsync(varaBalanceOfFunction) |> runNow
            ethBalance, varaBalance)

    Enumerable.Repeat((),10000)
    |> Seq.map (fun _ -> 
        takeRandomActionAsRandomAccount 
            randomAccounts 
            varaTokenService.ContractHandler.ContractAddress 
            simpleStakingService.ContractHandler.ContractAddress)
    |> Seq.toList
    |> ignore