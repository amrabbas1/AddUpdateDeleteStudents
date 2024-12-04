module StudentsDb

open Newtonsoft.Json
open System.IO
open Models

let jsonFilePath = "students.json"

// Load students from a JSON file
let loadStudents() =
    if File.Exists(jsonFilePath) then
        let json = File.ReadAllText(jsonFilePath)
        JsonConvert.DeserializeObject<Student list>(json)
    else
        []

// Save the students list to a JSON file
let saveStudents (students: Student list) =
    let json = JsonConvert.SerializeObject(students, Formatting.Indented)
    File.WriteAllText(jsonFilePath, json)

// Add a student to the database
let addStudent (student: Student) (db: Student list) = student :: db

// Edit a student in the database
let editStudent (id: int) (newStudent: Student) (db: Student list) =
    db |> List.map (fun student -> if student.ID = id then newStudent else student)

// Remove a student from the database
let removeStudent (id: int) (db: Student list) =
    db |> List.filter (fun student -> student.ID <> id)

