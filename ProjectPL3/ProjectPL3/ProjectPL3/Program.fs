open System
open System.Windows
open System.Windows.Forms
open System.Drawing
open UI


// Create the main form for the application

// Main application entry point
let main() =
    let form = createForm()
    try
        Application.Run(form)
    with
    | :? InvalidOperationException -> printfn "Invalid Operation"
    | (ex: exn) -> printfn "exception occurred: %s" ex.Message

main() 