open Nethereum.Web3
open SimpleStaking.Contracts.VaraToken
open SimpleStaking.Contracts.VaraToken.ContractDefinition
open SimpleStaking.Contracts.SimpleStaking
open SimpleStaking.Contracts.SimpleStaking.ContractDefinition
open System.Threading.Tasks
open TestBase
open System.Linq

printfn "Hello from F#"

let useContractAtRandon = 
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
    let simpleStakingDeployment = SimpleStakingDeployment(varaTokenService.ContractHandler.ContractAddress)
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
    randomAccounts
    |> List.map (fun acc -> 
        ethConn.SendEther acc.Address (bigInt 1000000000000000000UL)
        |> ignore

        ethConn.SendTxAsync(
            varaTokenService.ContractHandler.ContractAddress, 
            bigInt 1000000000000000000UL, 
            varaTokenService.ContractHandler.GetFunction("mint").CreateTransactionInput(acc.Address, bigInt 1000000000000000000UL))


        ethConn.Web3.Eth
            .GetEtherTransferService()
            .TransferEtherAndWaitForReceiptAsync(
                acc.Address, 
                gas = ethConn.Gas, 
                gasPriceGwei = ethConn.GasPrice, 
                etherAmount = bigInt 1000000000000000000UL) 
        |> runNow
        |> ignore

        varaTokenService
            .TransferRequestAsync(
                 TransferFunction(
                    To = "0x123456",
                    Amount = 1234567890I))
        |> runNow
        |> ignore)
    |> ignore

    while true do
        let acc = pickRandom randomAccounts
        let stakingOrUnStaking = (rnd.Next(0, 1) = 0)
        if stakingOrUnStaking then



    1
