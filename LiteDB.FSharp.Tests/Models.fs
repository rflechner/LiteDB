namespace LiteDB.FSharp.Tests.Models

[<CLIMutableAttribute>]
type ClientCustomer =
  { Id: int;
    Name: string
    Phones: string list }
  static member Build (id, name, phones) =
    { Id = id
      Name = name
      Phones = phones |> Array.toList }

