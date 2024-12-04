module UI
open System
open System.Windows.Forms
open System.Drawing
open Models
open Utilities
open StudentsDb


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
            saveStudents students
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
        let idInput = createInputDialog "Enter ID of the student to edit:" ""
        match idInput with
        | "" -> ignore (MessageBox.Show("No ID entered!"))
        | idInput ->
            let id = int idInput
            match List.tryFind (fun s -> s.ID = id) students with
            | Some s ->
                let nameInput = createInputDialog "Enter New Name:" s.Name
                let gradesInput = createInputDialog "Enter New Grades (comma-separated):" (String.Join(",", s.Grades))
                
                let newName = nameInput
                let newGrades = gradesInput.Split([|','|]) |> Array.map int |> Array.toList
                let editedStudent = { ID = s.ID; Name = newName; Grades = newGrades }

                students <- editStudent id editedStudent students
                saveStudents students
                ignore (MessageBox.Show("Student Edited!"))
            | None -> ignore (MessageBox.Show("Student not found!"))
    )
    form.Controls.Add(editButton)

    // Remove Student button
    let removeButton = new Button(Text = "Remove Student", Location = System.Drawing.Point(50, 180), Size = System.Drawing.Size(110, 30))
    removeButton.Click.Add(fun _ ->
        let idInput = createInputDialog "Enter ID of the student to remove:" ""
        if idInput <> "" then
            let id = int idInput
            students <- removeStudent id students
            saveStudents students
            ignore (MessageBox.Show("Student Removed!"))
    )
    form.Controls.Add(removeButton)

    // Show Students button
    let showButton = new Button(Text = "Show Students", Location = System.Drawing.Point(50, 220), Size = System.Drawing.Size(110, 30))
    showButton.Click.Add(fun _ ->
        let studentList = students |> List.map (fun s -> sprintf "ID: %d, Name: %s, Grades: %A" s.ID s.Name s.Grades)
        ignore (MessageBox.Show(String.Join("\n", studentList)))
    )
    form.Controls.Add(showButton)

    // Class Statistics button
    let statsButton = new Button(Text = "Class Statistics", Location = System.Drawing.Point(200, 100), Size = System.Drawing.Size(140, 30))
    statsButton.Click.Add(fun _ ->
        let stats = calculateClassStatistics 50.0 students
        ignore (MessageBox.Show(stats))
    )
    form.Controls.Add(statsButton)
      // Return the form
    form

