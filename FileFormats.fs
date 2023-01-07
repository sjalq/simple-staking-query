module FileFormats

open System
open System.Numerics
open System.IO

let toFile map =
    let timestamp = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")
    let filePath = sprintf "output/results %s.csv" timestamp

    File.WriteAllLines(
        filePath,
        map
        |> Map.map (fun k v -> sprintf "%A,%A" k v)
        |> Map.values
    )

    let merkleFormat =
        {| 
            decimals = 18 
            airdrop = map
        |}

    // write the config.json file
    let merkleFormatJson = Newtonsoft.Json.JsonConvert.SerializeObject(merkleFormat, Newtonsoft.Json.Formatting.Indented)
    let filePath = "output/config.json"
    File.WriteAllText(filePath, merkleFormatJson)

    let ts = @"// Types
type IConfig = {
  decimals: number;
  airdrop: Record<string, number>;
};

json = << add your config here >>

// Config from generator - Original format
// const config: IConfig = {
//   decimals: 18,
//   'airdrop': {
//     '0x016C8780e5ccB32E5CAA342a926794cE64d9C364': 10,
//     '0x185a4dc360ce69bdccee33b3784b0282f7961aea': 100,
//   },
// };

function convertJsonToIConfig(json: any): IConfig {
  return {
    decimals: Number(json.decimals),
    airdrop: json.airdrop
  };
}

const config = convertJsonToIConfig(json);

// Export config
export default config;
"

    // write the config.ts file
    let merkleFormatTs = ts.Replace("<< add your config here >>", merkleFormatJson)
    let filePath = "output/config.ts"
    File.WriteAllText(filePath, merkleFormatTs)