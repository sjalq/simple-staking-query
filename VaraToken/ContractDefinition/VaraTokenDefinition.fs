namespace SimpleStaking.Contracts.VaraToken.ContractDefinition

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

    
    
    type VaraTokenDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "60806040523480156200001157600080fd5b50604051806040016040528060048152602001635661726160e01b815250604051806040016040528060048152602001635641524160e01b8152508160039080519060200190620000649291906200018a565b5080516200007a9060049060208401906200018a565b5050506200009c336c0c9f2c9cd04674edea40000000620000a260201b60201c565b62000294565b6001600160a01b038216620000fd5760405162461bcd60e51b815260206004820152601f60248201527f45524332303a206d696e7420746f20746865207a65726f206164647265737300604482015260640160405180910390fd5b806002600082825462000111919062000230565b90915550506001600160a01b038216600090815260208190526040812080548392906200014090849062000230565b90915550506040518181526001600160a01b038316906000907fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef9060200160405180910390a35050565b828054620001989062000257565b90600052602060002090601f016020900481019282620001bc576000855562000207565b82601f10620001d757805160ff191683800117855562000207565b8280016001018555821562000207579182015b8281111562000207578251825591602001919060010190620001ea565b506200021592915062000219565b5090565b5b808211156200021557600081556001016200021a565b600082198211156200025257634e487b7160e01b600052601160045260246000fd5b500190565b600181811c908216806200026c57607f821691505b602082108114156200028e57634e487b7160e01b600052602260045260246000fd5b50919050565b6108ce80620002a46000396000f3fe608060405234801561001057600080fd5b50600436106100a95760003560e01c80633950935111610071578063395093511461012357806370a082311461013657806395d89b411461015f578063a457c2d714610167578063a9059cbb1461017a578063dd62ed3e1461018d57600080fd5b806306fdde03146100ae578063095ea7b3146100cc57806318160ddd146100ef57806323b872dd14610101578063313ce56714610114575b600080fd5b6100b66101c6565b6040516100c3919061070b565b60405180910390f35b6100df6100da36600461077c565b610258565b60405190151581526020016100c3565b6002545b6040519081526020016100c3565b6100df61010f3660046107a6565b610270565b604051601281526020016100c3565b6100df61013136600461077c565b610294565b6100f36101443660046107e2565b6001600160a01b031660009081526020819052604090205490565b6100b66102d3565b6100df61017536600461077c565b6102e2565b6100df61018836600461077c565b610379565b6100f361019b366004610804565b6001600160a01b03918216600090815260016020908152604080832093909416825291909152205490565b6060600380546101d590610837565b80601f016020809104026020016040519081016040528092919081815260200182805461020190610837565b801561024e5780601f106102235761010080835404028352916020019161024e565b820191906000526020600020905b81548152906001019060200180831161023157829003601f168201915b5050505050905090565b600033610266818585610387565b5060019392505050565b60003361027e8582856104ab565b61028985858561053d565b506001949350505050565b3360008181526001602090815260408083206001600160a01b038716845290915281205490919061026690829086906102ce908790610872565b610387565b6060600480546101d590610837565b3360008181526001602090815260408083206001600160a01b03871684529091528120549091908381101561036c5760405162461bcd60e51b815260206004820152602560248201527f45524332303a2064656372656173656420616c6c6f77616e63652062656c6f77604482015264207a65726f60d81b60648201526084015b60405180910390fd5b6102898286868403610387565b60003361026681858561053d565b6001600160a01b0383166103e95760405162461bcd60e51b8152602060048201526024808201527f45524332303a20617070726f76652066726f6d20746865207a65726f206164646044820152637265737360e01b6064820152608401610363565b6001600160a01b03821661044a5760405162461bcd60e51b815260206004820152602260248201527f45524332303a20617070726f766520746f20746865207a65726f206164647265604482015261737360f01b6064820152608401610363565b6001600160a01b0383811660008181526001602090815260408083209487168084529482529182902085905590518481527f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925910160405180910390a3505050565b6001600160a01b038381166000908152600160209081526040808320938616835292905220546000198114610537578181101561052a5760405162461bcd60e51b815260206004820152601d60248201527f45524332303a20696e73756666696369656e7420616c6c6f77616e63650000006044820152606401610363565b6105378484848403610387565b50505050565b6001600160a01b0383166105a15760405162461bcd60e51b815260206004820152602560248201527f45524332303a207472616e736665722066726f6d20746865207a65726f206164604482015264647265737360d81b6064820152608401610363565b6001600160a01b0382166106035760405162461bcd60e51b815260206004820152602360248201527f45524332303a207472616e7366657220746f20746865207a65726f206164647260448201526265737360e81b6064820152608401610363565b6001600160a01b0383166000908152602081905260409020548181101561067b5760405162461bcd60e51b815260206004820152602660248201527f45524332303a207472616e7366657220616d6f756e7420657863656564732062604482015265616c616e636560d01b6064820152608401610363565b6001600160a01b038085166000908152602081905260408082208585039055918516815290812080548492906106b2908490610872565b92505081905550826001600160a01b0316846001600160a01b03167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040516106fe91815260200190565b60405180910390a3610537565b600060208083528351808285015260005b818110156107385785810183015185820160400152820161071c565b8181111561074a576000604083870101525b50601f01601f1916929092016040019392505050565b80356001600160a01b038116811461077757600080fd5b919050565b6000806040838503121561078f57600080fd5b61079883610760565b946020939093013593505050565b6000806000606084860312156107bb57600080fd5b6107c484610760565b92506107d260208501610760565b9150604084013590509250925092565b6000602082840312156107f457600080fd5b6107fd82610760565b9392505050565b6000806040838503121561081757600080fd5b61082083610760565b915061082e60208401610760565b90509250929050565b600181811c9082168061084b57607f821691505b6020821081141561086c57634e487b7160e01b600052602260045260246000fd5b50919050565b6000821982111561089357634e487b7160e01b600052601160045260246000fd5b50019056fea2646970667358221220f8b492bf73780e2b83152bc57952a4a8ceb5b87f2d4f0e4b5671b058ae7758bc64736f6c634300080b0033"
        
        new() = VaraTokenDeployment(BYTECODE)
        

        
    
    [<Function("allowance", "uint256")>]
    type AllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "owner", 1)>]
            member val public Owner = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "spender", 2)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
        
    
    [<Function("approve", "bool")>]
    type ApproveFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 2)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("balanceOf", "uint256")>]
    type BalanceOfFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "account", 1)>]
            member val public Account = Unchecked.defaultof<string> with get, set
        
    
    [<Function("decimals", "uint8")>]
    type DecimalsFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("decreaseAllowance", "bool")>]
    type DecreaseAllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "subtractedValue", 2)>]
            member val public SubtractedValue = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("increaseAllowance", "bool")>]
    type IncreaseAllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "addedValue", 2)>]
            member val public AddedValue = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("name", "string")>]
    type NameFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("symbol", "string")>]
    type SymbolFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("totalSupply", "uint256")>]
    type TotalSupplyFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("transfer", "bool")>]
    type TransferFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "to", 1)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 2)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("transferFrom", "bool")>]
    type TransferFromFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "from", 1)>]
            member val public From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "to", 2)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 3)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Approval")>]
    type ApprovalEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "owner", 1, true )>]
            member val Owner = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "spender", 2, true )>]
            member val Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Transfer")>]
    type TransferEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "from", 1, true )>]
            member val From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "to", 2, true )>]
            member val To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type AllowanceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    [<FunctionOutput>]
    type BalanceOfOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type DecimalsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint8", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte> with get, set
        
    
    
    
    
    
    [<FunctionOutput>]
    type NameOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("string", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type SymbolOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("string", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type TotalSupplyOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    


