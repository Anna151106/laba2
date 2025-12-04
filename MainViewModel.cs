using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MauiApp3
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _xmlPath;
        private string _xslPath;
        private string _xslFileName = "Файл стилів не обрано";

        public string XslFileName
        {
            get => _xslFileName;
            set { _xslFileName = value; OnPropertyChanged(); }
        }

        private IXmlSearchStrategy _searchStrategy;

       
        public ObservableCollection<string> AllFaculties { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> AllCourses { get; set; } = new ObservableCollection<string>();

        
        private string _selectedFaculty;
        public string SelectedFaculty
        {
            get => _selectedFaculty;
            set { _selectedFaculty = value; OnPropertyChanged(); }
        }

        private string _selectedCourse;
        public string SelectedCourse
        {
            get => _selectedCourse;
            set { _selectedCourse = value; OnPropertyChanged(); }
        }

        
        public ObservableCollection<Student> SearchResults { get; set; } = new ObservableCollection<Student>();

        
        public List<string> StrategyNames { get; } = new List<string> { "DOM", "SAX", "LINQ" };

        private string _selectedStrategyName = "LINQ";
        public string SelectedStrategyName
        {
            get => _selectedStrategyName;
            set { _selectedStrategyName = value; OnPropertyChanged(); }
        }

       
        public ICommand LoadXslCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand TransformCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
         
            LoadDataCommand = new Command(async () => await LoadData());
            LoadXslCommand = new Command(async () => await LoadXsl());
            SearchCommand = new Command(PerformSearch);
            ClearCommand = new Command(ClearForm);
            TransformCommand = new Command(PerformTransformation);
            ExitCommand = new Command(async () => await ExitApp());
        }

        private async Task LoadData()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    _xmlPath = result.FullPath;
                   
                    _xslPath = _xmlPath.Replace(".xml", ".xsl");

                    ParseFileAttributesForFilters();
                    await Application.Current.MainPage.DisplayAlert("Успіх", "Файл завантажено", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

       
        private void ParseFileAttributesForFilters()
        {
            if (string.IsNullOrEmpty(_xmlPath)) return;

            AllFaculties.Clear();
            AllCourses.Clear();

          
            var doc = XDocument.Load(_xmlPath);

           
            var faculties = doc.Descendants("Student")
                               .Select(x => (string)x.Attribute("Faculty"))
                               .Distinct()
                               .Where(x => x != null);

            var courses = doc.Descendants("Student")
                             .Select(x => (string)x.Attribute("Course"))
                             .Distinct()
                             .Where(x => x != null);

            foreach (var f in faculties) AllFaculties.Add(f);
            foreach (var c in courses) AllCourses.Add(c);
        }

        private void PerformSearch()
        {
            if (string.IsNullOrEmpty(_xmlPath)) return;

         
            switch (SelectedStrategyName)
            {
                case "DOM": _searchStrategy = new DomSearchStrategy(); break;
                case "SAX": _searchStrategy = new SaxSearchStrategy(); break;
                case "LINQ": _searchStrategy = new LinqSearchStrategy(); break;
                default: _searchStrategy = new LinqSearchStrategy(); break;
            }

            
            var criteria = new SearchCriteria
            {
                Faculty = SelectedFaculty,
                Course = SelectedCourse
            };

            var results = _searchStrategy.Search(_xmlPath, criteria);

            SearchResults.Clear();
            foreach (var s in results)
            {
                SearchResults.Add(s);
            }
        }

        private void ClearForm()
        {
            SelectedFaculty = null;
            SelectedCourse = null;
            SearchResults.Clear();
        }

        private async Task LoadXsl()
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.xml" } },
                        { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                        { DevicePlatform.WinUI, new[] { ".xsl", ".xslt" } },
                        { DevicePlatform.macOS, new[] { "public.xml" } },
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Оберіть файл стилів (XSL)",
                    FileTypes = customFileType,
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    _xslPath = result.FullPath;
                    XslFileName = result.FileName;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void PerformTransformation()
        {
            if (string.IsNullOrEmpty(_xmlPath))
            {
                await Application.Current.MainPage.DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_xslPath))
            {
                await Application.Current.MainPage.DisplayAlert("Увага", "Спочатку завантажте XSL файл!", "OK");
                return;
            }

            if (!SearchResults.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Увага", "Спочатку виконайте пошук, щоб отримати дані для трансформації!", "OK");
                return;
            }

            try
            {
                
                var filteredXml = CreateXmlFromSearchResults();

                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(_xslPath);

                string htmlPath = Path.Combine(Path.GetTempPath(), $"report_{DateTime.Now.Ticks}.html");

                using (var writer = XmlWriter.Create(htmlPath, xslt.OutputSettings))
                {
                 
                    xslt.Transform(filteredXml.CreateNavigator(), writer);
                }

                await Application.Current.MainPage.DisplayAlert("HTML створено", $"Звіт збережено у тимчасовому файлі. Відкриваю...", "OK");
                await Launcher.Default.OpenAsync(new OpenFileRequest("Результат", new ReadOnlyFile(htmlPath)));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка трансформації", ex.Message, "OK");
            }
        }
      
        private XDocument CreateXmlFromSearchResults()
        {
            var root = new XElement("University");

            foreach (var student in SearchResults)
            {
                var studentElement = new XElement("Student",
                    new XAttribute("Name", student.Name ?? string.Empty),
                    new XAttribute("Faculty", student.Faculty ?? string.Empty),
                    new XAttribute("Course", student.Course ?? string.Empty)
                );

                
                var subjectDetails = student.ResultDetails?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

                foreach (var detail in subjectDetails)
                {
                 
                    var parts = detail.Split(new[] { ": " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var subjectName = parts[0].Trim();
                        var grade = parts[1].Trim();

                        studentElement.Add(new XElement("Subject",
                            new XAttribute("Name", subjectName),
                            new XAttribute("Grade", grade)
                        ));
                    }
                }

                root.Add(studentElement);
            }

            return new XDocument(root);
        }
        private async Task ExitApp()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (answer)
            {
                Application.Current.Quit();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
