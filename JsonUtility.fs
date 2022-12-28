module JsonUtility

open System.Reflection

let toJson obj =
    let t = obj.GetType()
    let props = t.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
                 |> Array.map (fun p -> p.Name, p.GetValue(obj))
                 |> dict
    Newtonsoft.Json.JsonConvert.SerializeObject(props, Newtonsoft.Json.Formatting.Indented)
