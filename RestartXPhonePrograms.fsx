module Process =
    let start (path: string) =
        System.Diagnostics.Process.Start path
        |> ignore

    let get pid =
        System.Diagnostics.Process.GetProcessById pid

    let kill (proc: System.Diagnostics.Process) =
        proc.Kill()

    let filename (proc: System.Diagnostics.Process) =
        try
            match proc.MainModule.FileName with
            null -> None
            | d -> Some d
        with _ ->
            None

module IO =
    let combine p1 p2 = System.IO.Path.Combine(p1, p2)

    let prog86 =
        System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.ProgramFilesX86
        )

    let enumerateFiles filter directory =
        System.IO.Directory.EnumerateFiles(directory, filter)

    let enumerateExeFiles =
        enumerateFiles "*.exe"

let (@@) = IO.combine

let xphoneDir = IO.prog86 @@ @"XPhone40"
let xphoneTeamPanelDir = IO.prog86 @@ @"XPhoneTeam30"

let xPhoneCommander, xPhoneTeamPanel =
    (xphoneDir @@ "XPhone.exe",
     xphoneTeamPanelDir @@ @"XPhoneTeam.exe")

let processesToKill =
    Seq.concat [
        IO.enumerateExeFiles xphoneDir
        IO.enumerateExeFiles xphoneTeamPanelDir
    ]
    |> Set.ofSeq

let xPhoneProcesses =
    System.Diagnostics.Process.GetProcesses()
    |> Seq.choose (fun p ->
        Process.filename p
        |> Option.map (fun name -> p.Id, name))
    |> Seq.filter (snd >> processesToKill.Contains)

printfn "Killing all (old) XPhone processes:"
xPhoneProcesses
|> Seq.iter (fun (pid, name) ->
    printfn "* %s" name
    Process.kill (Process.get pid))

System.Threading.Thread.Sleep(1000)

printfn "Starting XPhone Commander"
Process.start xPhoneCommander

System.Threading.Thread.Sleep(1000)

printfn "Starting XPhone Team Panel"
Process.start xPhoneTeamPanel

