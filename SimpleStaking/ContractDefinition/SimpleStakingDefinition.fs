namespace SimpleStaking.Contracts.SimpleStaking.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts
open System.Threading

    
    
    type SimpleStakingDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "608060405234801561001057600080fd5b5060405161056938038061056983398101604081905261002f91610054565b600080546001600160a01b0319166001600160a01b0392909216919091179055610084565b60006020828403121561006657600080fd5b81516001600160a01b038116811461007d57600080fd5b9392505050565b6104d6806100936000396000f3fe608060405234801561001057600080fd5b50600436106100575760003560e01c8063227a473b1461005c5780637538eecf146100715780639b2d155414610079578063bc7b286d146100ac578063fd84b53b146100cc575b600080fd5b61006f61006a3660046103f0565b6100f7565b005b61006f6101a4565b610099610087366004610409565b60026020526000908152604090205481565b6040519081526020015b60405180910390f35b6100996100ba366004610409565b60016020526000908152604090205481565b6000546100df906001600160a01b031681565b6040516001600160a01b0390911681526020016100a3565b33600090815260016020526040902054681043561a88293000009061011c908361044f565b101561016f5760405162461bcd60e51b815260206004820152601960248201527f7374616b65206d757374206265203e3d2033303020766172610000000000000060448201526064015b60405180910390fd5b336000908152600160205260409020546101896000610206565b6101a0818361019b426276a70061044f565b6102f7565b5050565b336000908152600260205260409020544210156101fa5760405162461bcd60e51b815260206004820152601460248201527363616e6e6f7420756e7374616b65206561726c7960601b6044820152606401610166565b6102046001610206565b565b33600081815260016020908152604080832054600283529281902054905183815292934293919290917f204fccf0d92ed8d48f204adb39b2e81e92bad0dedb93f5716ca9478cfb57de00910160405180910390a4336000908152600160209081526040808320839055600290915281205581156101a05760005460405163a9059cbb60e01b8152336004820152602481018390526001600160a01b039091169063a9059cbb906044016020604051808303816000875af11580156102ce573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906102f29190610467565b505050565b610301828461044f565b33600081815260016020908152604080832094909455600290528281208490555491516323b872dd60e01b81526004810191909152306024820152604481018490526001600160a01b03909116906323b872dd906064016020604051808303816000875af1158015610377573d6000803e3d6000fd5b505050506040513d601f19601f8201168201806040525081019061039b9190610467565b5080337fb4caaf29adda3eefee3ad552a8e85058589bf834c7466cae4ee58787f70589ed6103c9858761044f565b6103d34286610489565b6040805192835260208301919091520160405180910390a3505050565b60006020828403121561040257600080fd5b5035919050565b60006020828403121561041b57600080fd5b81356001600160a01b038116811461043257600080fd5b9392505050565b634e487b7160e01b600052601160045260246000fd5b6000821982111561046257610462610439565b500190565b60006020828403121561047957600080fd5b8151801515811461043257600080fd5b60008282101561049b5761049b610439565b50039056fea2646970667358221220ceef659cbf2e9317ba2cd60e254915835391b4a5696be68d1706c6415a42bec964736f6c634300080b0033"
        
        new() = SimpleStakingDeployment(BYTECODE)
        
            [<Parameter("address", "_vara", 1)>]
            member val public Vara = Unchecked.defaultof<string> with get, set
        
    
    [<Function("Stake")>]
    type StakeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "_amount", 1)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("Unstake")>]
    type UnstakeFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("releaseDates", "uint256")>]
    type ReleaseDatesFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("stakedFunds", "uint256")>]
    type StakedFundsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("vara", "address")>]
    type VaraFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Event("Staked")>]
    type StakedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_staker", 1, true )>]
            member val Staker = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_amount", 2, false )>]
            member val Amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_duration", 3, false )>]
            member val Duration = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_releaseDate", 4, true )>]
            member val ReleaseDate = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Unstaked")>]
    type UnstakedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_staker", 1, true )>]
            member val Staker = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_amount", 2, false )>]
            member val Amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_originalReleaseDate", 3, true )>]
            member val OriginalReleaseDate = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_releaseDate", 4, true )>]
            member val ReleaseDate = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    [<FunctionOutput>]
    type ReleaseDatesOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type StakedFundsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type VaraOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
    

