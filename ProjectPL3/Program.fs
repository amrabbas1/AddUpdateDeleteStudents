open System
open System.Windows
open System.Windows.Forms
open Newtonsoft.Json
open System.IO

// Define the Student Record
type Student = {
    ID: int
    Name: string
    Grades: int list
}

// Path to the JSON file for storing student data
let jsonFilePath = "students.json"

// Function to load the students from the JSON file
let loadStudents() =
    if File.Exists(jsonFilePath) then
        let json = File.ReadAllText(jsonFilePath)
        JsonConvert.DeserializeObject<Student list>(json)
    else
        []  // Return an empty list if the file doesn't exist

// Function to save the students to the JSON file
let saveStudents (students: Student list) =
    let json = JsonConvert.SerializeObject(students, Formatting.Indented)
    File.WriteAllText(jsonFilePath, json)

// Add a student to the database
let addStudent (student: Student) (db: Student list) =
    student :: db

// Edit a student's information
let editStudent (id: int) (newStudent: Student) (db: Student list) =
    db |> List.map (fun student -> if student.ID = id then newStudent else student)

// Remove a student from the database
let removeStudent (id: int) (db: Student list) =
    db |> List.filter (fun student -> student.ID <> id)

// Create the InputBox dialog
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
        ""  // Return an empty string if cancelled

// Create the main application form
let createForm() =
    let form = new Form()
    form.Text <- "Student Grades Management System"
    form.ClientSize <- System.Drawing.Size(400, 400)

    // Label for instructions
    let label = new Label()
    label.Text <- "Welcome to Student Grades Management"
    label.Location <- System.Drawing.Point(100, 20)
    label.Size <- System.Drawing.Size(300, 40)
    form.Controls.Add(label)

    // Load the students from the file
    let mutable students = loadStudents()

    // Input fields for adding or editing students
    let idTextBox = new TextBox(Location = System.Drawing.Point(150, 80), Size = System.Drawing.Size(200, 20))
    let nameTextBox = new TextBox(Location = System.Drawing.Point(150, 120), Size = System.Drawing.Size(200, 20))
    let gradesTextBox = new TextBox(Location = System.Drawing.Point(150, 160), Size = System.Drawing.Size(200, 20))
    
    // Add Student button
    let addButton = new Button(Text = "Add Student", Location = System.Drawing.Point(50, 100), Size = System.Drawing.Size(110, 30))
    addButton.Click.Add(fun _ ->
        // Show dialog to get student details
        let idInput = createInputDialog "Enter Student ID:" ""
        let nameInput = createInputDialog "Enter Student Name:" ""
        let gradesInput = createInputDialog "Enter Student Grades (comma-separated):" ""
        
        if idInput <> "" && nameInput <> "" && gradesInput <> "" then
            let id = int idInput
            let name = nameInput
            let grades = gradesInput.Split([|','|]) |> Array.map int |> Array.toList
            
            let newStudent = { ID = id; Name = name; Grades = grades }
            students <- addStudent newStudent students
            saveStudents(students)  // Save the updated list to the JSON file
            ignore (MessageBox.Show("Student Added!"))
    )
    form.Controls.Add(addButton)

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
