open System
open System.Windows
open System.Windows.Forms
open Newtonsoft.Json
open System.IO

type Student = {
    ID: int
    Name: string
    Grades: int list
}

let jsonFilePath = "students.json"

let loadStudents() =
    if File.Exists(jsonFilePath) then
        let json = File.ReadAllText(jsonFilePath)
        JsonConvert.DeserializeObject<Student list>(json)
    else
        []

let saveStudents (students: Student list) =
    let json = JsonConvert.SerializeObject(students, Formatting.Indented)
    File.WriteAllText(jsonFilePath, json)

let addStudent (student: Student) (db: Student list) = student :: db

let editStudent (id: int) (newStudent: Student) (db: Student list) =
    db |> List.map (fun student -> if student.ID = id then newStudent else student)

let removeStudent (id: int) (db: Student list) =
    db |> List.filter (fun student -> student.ID <> id)

let createInputDialog (prompt: string) (defaultValue: string) =
    let form = new Form()
    form.Text <- "Input"
    form.ClientSize <- System.Drawing.Size(300, 150)
    
    let label = new Label()
    label.Text <- prompt
    label.Location <- System.Drawing.Point(10, 10)
    label.Size <- System.Drawing.Size(300, 40)
    form.Controls.Add(label)

    let textBox = new TextBox()
    textBox.Text <- defaultValue
    textBox.Location <- System.Drawing.Point(10, 50)
    form.Controls.Add(textBox)

    let button = new Button()
    button.Text <- "OK"
    button.Location <- System.Drawing.Point(10, 80)
    button.DialogResult <- DialogResult.OK
    form.Controls.Add(button)
    
    form.AcceptButton <- button
    form.StartPosition <- FormStartPosition.CenterScreen

    if form.ShowDialog() = DialogResult.OK then
        textBox.Text
    else
        ""

let getStudentAverage (id: int) (db: Student list) =
    match db |> List.tryFind (fun s -> s.ID = id) with
    | Some student ->
        if List.isEmpty student.Grades then
            0.0  // Return 0.0 if no grades are available
        else
            (float (List.sum student.Grades)) / (float (List.length student.Grades))
    | None -> 
        -1.0  // Return -1.0 to indicate student not found
let calculateClassStatistics (passThreshold: float) (db: Student list) =
    match db with
    | [] -> 
        "No students in the database."
    | _ -> 
        // Calculate statistics for each student
        let stats = 
            db |> List.map (fun student ->
                let avg = 
                    match student.Grades with
                    | [] -> 0.0  // No grades available
                    | _ -> 
                        (float (List.sum student.Grades)) / (float (List.length student.Grades))
                (student, avg >= passThreshold))
        
        // Total students
        let totalStudents = List.length db

        // Count passed and failed
        let passed, failed =
            stats |> List.fold (fun (p, f) (_, isPass) ->
                if isPass then (p + 1, f) else (p, f + 1)) (0, 0)
        
        // Calculate pass and fail rates
        let passRate = (float passed / float totalStudents) * 100.0
        let failRate = (float failed / float totalStudents) * 100.0

        // Create the result string
        sprintf "Class Statistics:\nTotal Students: %d\nPassed: %d (%.2f%%)\nFailed: %d (%.2f%%)" 
                totalStudents passed passRate failed failRate
let findHighestAndLowestAverages (db: Student list) =
    match db with
    | [] -> 
        None  // Return None if the database is empty
    | _ ->
        // Compute averages for each student
        let averages =
            db
            |> List.map (fun student -> 
                if student.Grades = [] then 0.0 // Handle empty grade lists
                else float (List.sum student.Grades) / float (List.length student.Grades))
        
        match averages with
        | [] -> 
            None  // No averages available
        | _ ->
            let highest = List.max averages
            let lowest = List.min averages
            Some (highest, lowest)  // Return a tuple with highest and lowest averages



let createForm() =
    let form = new Form()
    form.Text <- "Student Grades Management System"
    form.ClientSize <- System.Drawing.Size(400, 400)

    let mutable students = loadStudents()

    let label = new Label()
    label.Text <- "Welcome to Student Grades Management"
    label.Location <- System.Drawing.Point(100, 20)
    label.Size <- System.Drawing.Size(300, 40)
    form.Controls.Add(label)

    let addButton = new Button(Text = "Add Student", Location = System.Drawing.Point(50, 100), Size = System.Drawing.Size(110, 30))
    addButton.Click.Add(fun _ ->
        let idInput = createInputDialog "Enter Student ID:" ""
        let nameInput = createInputDialog "Enter Student Name:" ""
        let gradesInput = createInputDialog "Enter Student Grades (comma-separated):" ""
        
        if idInput <> "" && nameInput <> "" && gradesInput <> "" then
            let id = int idInput
            let name = nameInput
            let grades = gradesInput.Split([|','|]) |> Array.map int |> Array.toList
            
            let newStudent = { ID = id; Name = name; Grades = grades }
            students <- addStudent newStudent students
            saveStudents(students)
            ignore (MessageBox.Show("Student Added!"))
    )
    form.Controls.Add(addButton)

   // Calculate Average button
    let avgButton = new Button(Text = "Student Average", Location = System.Drawing.Point(50, 260), Size = System.Drawing.Size(140, 30))
    avgButton.Click.Add(fun _ ->
        let idInput = createInputDialog "Enter ID of the student:" ""
        if idInput <> "" then
            let id = int idInput
            let average = getStudentAverage id students
            if average >= 0.0 then
                ignore (MessageBox.Show(sprintf "Student ID: %d, Average Grade: %.2f" id average))
            else
                ignore (MessageBox.Show("Student not found!"))
    )
    form.Controls.Add(avgButton)
        // Edit Student button
    let editButton = new Button(Text = "Edit Student", Location = System.Drawing.Point(50, 140), Size = System.Drawing.Size(110, 30))
    editButton.Click.Add(fun _ ->
        // Show dialog to edit student details
        let idInput = createInputDialog "Enter ID of the student to edit:" ""
        let id = int idInput
        let student = students |> List.tryFind (fun s -> s.ID = id)

        match student with
        | Some s ->
            let nameInput = createInputDialog "Enter New Name:" s.Name
            let gradesInput = createInputDialog "Enter New Grades (comma-separated):" (String.Join(",", s.Grades))
            
            let newName = nameInput
            let newGrades = gradesInput.Split([|','|]) |> Array.map int |> Array.toList
            let editedStudent = { ID = s.ID; Name = newName; Grades = newGrades }

            students <- editStudent id editedStudent students
            saveStudents(students)
            ignore (MessageBox.Show("Student Edited!"))
        | None -> ignore (MessageBox.Show("Student not found!"))
    )
    form.Controls.Add(editButton)

    // Remove Student button
    let removeButton = new Button(Text = "Remove Student", Location = System.Drawing.Point(50, 180), Size = System.Drawing.Size(110, 30))
    removeButton.Click.Add(fun _ ->
        // Show dialog to remove student
        let idInput = createInputDialog "Enter ID of the student to remove:" ""
        let id = int idInput

        students <- removeStudent id students
        saveStudents(students)  // Save the updated list to the JSON file
        ignore (MessageBox.Show("Student Removed!"))
    )
    form.Controls.Add(removeButton)

    // Show Students button
    let showButton = new Button(Text = "Show Students", Location = System.Drawing.Point(50, 220), Size = System.Drawing.Size(110, 30))
    showButton.Click.Add(fun _ ->
        let studentList = students |> List.map (fun s -> sprintf "ID: %d, Name: %s, Grades: %A" s.ID s.Name s.Grades)
        ignore (MessageBox.Show(String.Join("\n", studentList)))  // Show student details in MessageBox
    )
    form.Controls.Add(showButton)

    // Class Statistics button
    let statsButton = new Button(Text = "Class Statistics", Location = System.Drawing.Point(50, 300), Size = System.Drawing.Size(140, 30))
    statsButton.Click.Add(fun _ ->
        let thresholdInput = createInputDialog "Enter Pass Threshold:" "50"
        if thresholdInput <> "" then
            let passThreshold = float thresholdInput
            let statsMessage = calculateClassStatistics passThreshold students
            ignore (MessageBox.Show(statsMessage))
    )
    form.Controls.Add(statsButton)
  // Highest and Lowest Grades button
    let avgGradesButton = new Button(Text = "Highest & Lowest Averages", Location = System.Drawing.Point(50, 340), Size = System.Drawing.Size(180, 30))
    avgGradesButton.Click.Add(fun _ ->
        match findHighestAndLowestAverages students with
        | None -> 
            ignore (MessageBox.Show("No students or grades available to analyze."))
        | Some (highest, lowest) -> 
            let message = sprintf "Highest Average Grade: %.2f\nLowest Average Grade: %.2f" highest lowest
            ignore (MessageBox.Show(message))
    )
    form.Controls.Add(avgGradesButton)
    // Return the form
    form




// Main application entry point
let main() =
    let form = createForm()
    try
        Application.Run(form)
    with
    | :? InvalidOperationException -> printfn "Invalid Operation"
    | (ex: exn) -> printfn "exception occurred: %s" ex.Message

main()
