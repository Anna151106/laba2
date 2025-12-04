using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MauiApp3
{
    public class LinqSearchStrategy : IXmlSearchStrategy
    {
        public List<Student> Search(string filePath, SearchCriteria criteria)
        {
            var doc = XDocument.Load(filePath);
            var results = new List<Student>();

            var students = doc.Descendants("Student").Where(s =>
                (string.IsNullOrEmpty(criteria.Faculty) || (string)s.Attribute("Faculty") == criteria.Faculty) &&
                (string.IsNullOrEmpty(criteria.Course) || (string)s.Attribute("Course") == criteria.Course)
            );

            foreach (var s in students)
            {
                var subjects = s.Elements("Subject").Where(sub =>
                     string.IsNullOrEmpty(criteria.SubjectName) || (string)sub.Attribute("Name") == criteria.SubjectName
                );

                if (subjects.Any())
                {
                    var studObj = new Student
                    {
                        Name = s.Attribute("Name")?.Value,
                        Faculty = s.Attribute("Faculty")?.Value,
                        Course = s.Attribute("Course")?.Value,
                        ResultDetails = string.Join(", ", subjects.Select(x => $"{x.Attribute("Name").Value}: {x.Attribute("Grade").Value}"))
                    };
                    results.Add(studObj);
                }
            }
            return results;
        }
    }


    public class DomSearchStrategy : IXmlSearchStrategy
    {
        public List<Student> Search(string filePath, SearchCriteria criteria)
        {
            var results = new List<Student>();
            var doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode node in doc.SelectNodes("//Student"))
            {
                string faculty = node.Attributes["Faculty"]?.Value;
                string course = node.Attributes["Course"]?.Value;

                
                bool matchFaculty = string.IsNullOrEmpty(criteria.Faculty) || faculty == criteria.Faculty;
                bool matchCourse = string.IsNullOrEmpty(criteria.Course) || course == criteria.Course;

                if (matchFaculty && matchCourse)
                {
                    var subjects = new List<string>();
                    foreach (XmlNode subNode in node.SelectNodes("Subject"))
                    {
                        string subName = subNode.Attributes["Name"]?.Value;
                        if (string.IsNullOrEmpty(criteria.SubjectName) || subName == criteria.SubjectName)
                        {
                            subjects.Add($"{subName}: {subNode.Attributes["Grade"]?.Value}");
                        }
                    }

                    if (subjects.Count > 0)
                    {
                        results.Add(new Student
                        {
                            Name = node.Attributes["Name"]?.Value,
                            Faculty = faculty,
                            Course = course,
                            ResultDetails = string.Join(", ", subjects)
                        });
                    }
                }
            }
            return results;
        }
    }

   
    public class SaxSearchStrategy : IXmlSearchStrategy
    {
        public List<Student> Search(string filePath, SearchCriteria criteria)
        {
            var results = new List<Student>();
            using (var reader = XmlReader.Create(filePath))
            {
                Student currentStudent = null;
                List<string> currentSubjects = null;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Student")
                    {
                        string name = reader.GetAttribute("Name");
                        string faculty = reader.GetAttribute("Faculty");
                        string course = reader.GetAttribute("Course");

                        bool matchFaculty = string.IsNullOrEmpty(criteria.Faculty) || faculty == criteria.Faculty;
                        bool matchCourse = string.IsNullOrEmpty(criteria.Course) || course == criteria.Course;

                        if (matchFaculty && matchCourse)
                        {
                            currentStudent = new Student { Name = name, Faculty = faculty, Course = course };
                            currentSubjects = new List<string>();
                        }
                        else
                        {
                            currentStudent = null; 
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Subject" && currentStudent != null)
                    {
                        string subName = reader.GetAttribute("Name");
                        string grade = reader.GetAttribute("Grade");

                        if (string.IsNullOrEmpty(criteria.SubjectName) || subName == criteria.SubjectName)
                        {
                            currentSubjects.Add($"{subName}: {grade}");
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student")
                    {
                        if (currentStudent != null && currentSubjects.Count > 0)
                        {
                            currentStudent.ResultDetails = string.Join(", ", currentSubjects);
                            results.Add(currentStudent);
                            currentStudent = null;
                        }
                    }
                }
            }
            return results;
        }
    }
}
