using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryShared.DTOs; // Ensure this namespace is correct for your DTOs

namespace LibraryClient
{
    public partial class Form1 : Form
    {
        private HttpClient _httpClient;
        private string _authToken;
        // IMPORTANT: Ensure this Base URL matches your LibraryWebAPI's HTTPS URL (from launchSettings.json)
        private string _baseUrl = "https://localhost:7053/api/";

        private TabControl _mainTabControl;
        private TabPage _booksTab;
        private TabPage _borrowersTab;
        private TabPage _loansTab;
        private TabPage _loginTab;

        public Form1()
        {
            InitializeComponent(); // Required for designer-generated code
            InitializeClient();
            InitializeUI();
        }

        private void InitializeClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void InitializeUI()
        {
            // Main form setup
            this.Text = "Library Management System";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main tab control
            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_mainTabControl);

            // Initialize login tab first
            _loginTab = new TabPage("Login");
            InitializeLoginTab();
            _mainTabControl.TabPages.Add(_loginTab);

            // Other tabs
            _booksTab = new TabPage("Books");
            InitializeBooksTab();
            _mainTabControl.TabPages.Add(_booksTab);

            _borrowersTab = new TabPage("Borrowers");
            InitializeBorrowersTab();
            _mainTabControl.TabPages.Add(_borrowersTab);

            _loansTab = new TabPage("Loans");
            InitializeLoansTab();
            _mainTabControl.TabPages.Add(_loansTab);

            // Disable all tabs except login initially
            SetTabsEnabled(false);
            _loginTab.Enabled = true; // Use the field reference instead of name lookup
            _mainTabControl.SelectedTab = _loginTab; // Ensure login tab is selected initially
        }

        private void SetTabsEnabled(bool enabled)
        {
            if (_mainTabControl == null || _mainTabControl.TabPages.Count == 0)
                return;

            foreach (TabPage tab in _mainTabControl.TabPages)
            {
                if (tab != _loginTab)
                {
                    tab.Enabled = enabled;
                }
            }
        }

        private async Task Login(string username, string password)
        {
            try
            {
                var loginDto = new UserLoginDTO
                {
                    Username = username,
                    Password = password
                };

                var content = new StringContent(JsonSerializer.Serialize(loginDto),
                                             Encoding.UTF8, "application/json");

                Cursor.Current = Cursors.WaitCursor; // Show loading indicator

                var response = await _httpClient.PostAsync("auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {response.StatusCode} - {errorContent}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(responseContent);

                _authToken = authResponse?.Token; // Use null-conditional operator for safety

                if (string.IsNullOrEmpty(_authToken))
                {
                    MessageBox.Show("Login successful, but no token received. Please check API response.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Display the token for debugging purposes
                MessageBox.Show($"Login successful! Token received: {_authToken.Substring(0, Math.Min(_authToken.Length, 50))}...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);

                SetTabsEnabled(true);
                _mainTabControl.SelectedTab = _booksTab; // Switch to books tab after login
                await LoadBooks(); // Load books after successful login and token set
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Cannot connect to server. Please ensure the API is running at {_baseUrl}.\n\nError: {ex.Message}",
                                  "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during login: {ex.Message}",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default; // Hide loading indicator
            }
        }

        private void ShowRegisterDialog()
        {
            var form = new Form
            {
                Text = "Register New User",
                Width = 400,
                Height = 350,
                StartPosition = FormStartPosition.CenterParent
            };

            var usernameLabel = new Label { Text = "Username:", Left = 20, Top = 20, Width = 100 };
            var usernameTextBox = new TextBox { Left = 130, Top = 20, Width = 200 };

            var fullNameLabel = new Label { Text = "Full Name:", Left = 20, Top = 60, Width = 100 };
            var fullNameTextBox = new TextBox { Left = 130, Top = 60, Width = 200 };

            var emailLabel = new Label { Text = "Email:", Left = 20, Top = 100, Width = 100 };
            var emailTextBox = new TextBox { Left = 130, Top = 100, Width = 200 };

            var passwordLabel = new Label { Text = "Password:", Left = 20, Top = 140, Width = 100 };
            var passwordTextBox = new TextBox { Left = 130, Top = 140, Width = 200, PasswordChar = '*' };

            var confirmLabel = new Label { Text = "Confirm Password:", Left = 20, Top = 180, Width = 100 };
            var confirmTextBox = new TextBox { Left = 130, Top = 180, Width = 200, PasswordChar = '*' };

            var registerButton = new Button { Text = "Register", Left = 130, Top = 220, Width = 80 };
            registerButton.Click += async (s, ev) =>
            {
                if (passwordTextBox.Text != confirmTextBox.Text)
                {
                    MessageBox.Show("Passwords do not match", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newUser = new UserRegisterDTO
                {
                    Username = usernameTextBox.Text,
                    FullName = fullNameTextBox.Text,
                    Email = emailTextBox.Text,
                    Password = passwordTextBox.Text
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("auth/register", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Registration failed: {response.StatusCode} - {errorContent}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("Registration successful. Please login with your new credentials.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var cancelButton = new Button { Text = "Cancel", Left = 220, Top = 220, Width = 80 };
            cancelButton.Click += (s, ev) => form.Close();

            form.Controls.AddRange(new Control[] { usernameLabel, usernameTextBox,
                fullNameLabel, fullNameTextBox, emailLabel, emailTextBox,
                passwordLabel, passwordTextBox, confirmLabel, confirmTextBox,
                registerButton, cancelButton });

            form.ShowDialog();
        }

        private async Task LoadBooks()
        {
            // Only attempt to load books if an authentication token is available
            if (string.IsNullOrEmpty(_authToken))
            {
                // Optionally show a message or just silently return,
                // as the tab is already disabled before login.
                MessageBox.Show("Please log in to view books.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync("books");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<BookDTO>>(content);

                var grid = (DataGridView)_booksTab.Controls[0].Controls["booksGrid"];
                grid.DataSource = books;
                grid.Columns["BookId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- Other data loading/management methods (Borrowers, Loans) should also have
        // --- a similar _authToken check at their beginning if they are protected. ---

        private void InitializeBooksTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            _booksTab.Controls.Add(panel);

            var dataGridView = new DataGridView
            {
                Name = "booksGrid",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            panel.Controls.Add(dataGridView);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(buttonPanel);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Left,
                Width = 100
            };
            refreshButton.Click += async (sender, e) => await LoadBooks();
            buttonPanel.Controls.Add(refreshButton);

            var addButton = new Button
            {
                Text = "Add Book",
                Dock = DockStyle.Left,
                Width = 100
            };
            addButton.Click += ShowAddBookDialog;
            buttonPanel.Controls.Add(addButton);

            var editButton = new Button
            {
                Text = "Edit Book",
                Dock = DockStyle.Left,
                Width = 100
            };
            editButton.Click += ShowEditBookDialog;
            buttonPanel.Controls.Add(editButton);

            var deleteButton = new Button
            {
                Text = "Delete Book",
                Dock = DockStyle.Left,
                Width = 100
            };
            deleteButton.Click += DeleteBook;
            buttonPanel.Controls.Add(deleteButton);
        }

        private void ShowAddBookDialog(object sender, EventArgs e)
        {
            var form = new Form
            {
                Text = "Add New Book",
                Width = 400,
                Height = 350,
                StartPosition = FormStartPosition.CenterParent
            };

            var titleLabel = new Label { Text = "Title:", Left = 20, Top = 20, Width = 100 };
            var titleTextBox = new TextBox { Left = 130, Top = 20, Width = 200 };

            var authorLabel = new Label { Text = "Author:", Left = 20, Top = 60, Width = 100 };
            var authorTextBox = new TextBox { Left = 130, Top = 60, Width = 200 };

            var isbnLabel = new Label { Text = "ISBN:", Left = 20, Top = 100, Width = 100 };
            var isbnTextBox = new TextBox { Left = 130, Top = 100, Width = 200 };

            var yearLabel = new Label { Text = "Published Year:", Left = 20, Top = 140, Width = 100 };
            var yearNumeric = new NumericUpDown { Left = 130, Top = 140, Width = 100, Minimum = 1000, Maximum = DateTime.Now.Year };

            var copiesLabel = new Label { Text = "Total Copies:", Left = 20, Top = 180, Width = 100 };
            var copiesNumeric = new NumericUpDown { Left = 130, Top = 180, Width = 100, Minimum = 1 };

            var saveButton = new Button { Text = "Save", Left = 130, Top = 220, Width = 80 };
            saveButton.Click += async (s, ev) =>
            {
                var newBook = new BookCreateDTO
                {
                    Title = titleTextBox.Text,
                    Author = authorTextBox.Text,
                    ISBN = isbnTextBox.Text,
                    PublishedYear = (int)yearNumeric.Value,
                    TotalCopies = (int)copiesNumeric.Value
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(newBook), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("books", content);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Book added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    await LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var cancelButton = new Button { Text = "Cancel", Left = 220, Top = 220, Width = 80 };
            cancelButton.Click += (s, ev) => form.Close();

            form.Controls.AddRange(new Control[] { titleLabel, titleTextBox, authorLabel, authorTextBox,
                isbnLabel, isbnTextBox, yearLabel, yearNumeric, copiesLabel, copiesNumeric,
                saveButton, cancelButton });

            form.ShowDialog();
        }

        private void ShowEditBookDialog(object sender, EventArgs e)
        {
            var grid = (DataGridView)_booksTab.Controls[0].Controls["booksGrid"];
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to edit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedBook = (BookDTO)grid.SelectedRows[0].DataBoundItem;
            var form = new Form
            {
                Text = "Edit Book",
                Width = 400,
                Height = 350,
                StartPosition = FormStartPosition.CenterParent
            };

            var titleLabel = new Label { Text = "Title:", Left = 20, Top = 20, Width = 100 };
            var titleTextBox = new TextBox { Left = 130, Top = 20, Width = 200, Text = selectedBook.Title };

            var authorLabel = new Label { Text = "Author:", Left = 20, Top = 60, Width = 100 };
            var authorTextBox = new TextBox { Left = 130, Top = 60, Width = 200, Text = selectedBook.Author };

            var isbnLabel = new Label { Text = "ISBN:", Left = 20, Top = 100, Width = 100 };
            var isbnTextBox = new TextBox { Left = 130, Top = 100, Width = 200, Text = selectedBook.ISBN };

            var yearLabel = new Label { Text = "Published Year:", Left = 20, Top = 140, Width = 100 };
            var yearNumeric = new NumericUpDown { Left = 130, Top = 140, Width = 100, Minimum = 1000, Maximum = DateTime.Now.Year, Value = selectedBook.PublishedYear };

            var copiesLabel = new Label { Text = "Total Copies:", Left = 20, Top = 180, Width = 100 };
            var copiesNumeric = new NumericUpDown { Left = 130, Top = 180, Width = 100, Minimum = 1, Value = selectedBook.TotalCopies };

            var saveButton = new Button { Text = "Save", Left = 130, Top = 220, Width = 80 };
            saveButton.Click += async (s, ev) =>
            {
                var updatedBook = new BookUpdateDTO
                {
                    Title = titleTextBox.Text,
                    Author = authorTextBox.Text,
                    ISBN = isbnTextBox.Text,
                    PublishedYear = (int)yearNumeric.Value,
                    TotalCopies = (int)copiesNumeric.Value
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(updatedBook), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PutAsync($"books/{selectedBook.BookId}", content);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Book updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    await LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var cancelButton = new Button { Text = "Cancel", Left = 220, Top = 220, Width = 80 };
            cancelButton.Click += (s, ev) => form.Close();

            form.Controls.AddRange(new Control[] { titleLabel, titleTextBox, authorLabel, authorTextBox,
                isbnLabel, isbnTextBox, yearLabel, yearNumeric, copiesLabel, copiesNumeric,
                saveButton, cancelButton });

            form.ShowDialog();
        }

        private async void DeleteBook(object sender, EventArgs e)
        {
            var grid = (DataGridView)_booksTab.Controls[0].Controls["booksGrid"];
            if (grid.SelectedRows.Count == 0) return;

            var book = (BookDTO)grid.SelectedRows[0].DataBoundItem;

            if (MessageBox.Show($"Delete {book.Title}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"books/{book.BookId}");
                    response.EnsureSuccessStatusCode();
                    await LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeLoginTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            _loginTab.Controls.Add(panel);

            var loginPanel = new Panel { Width = 300, Height = 200 };
            loginPanel.Left = (panel.Width - loginPanel.Width) / 2;
            loginPanel.Top = (panel.Height - loginPanel.Height) / 2;
            panel.Controls.Add(loginPanel);

            var usernameLabel = new Label { Text = "Username:", Left = 20, Top = 30, Width = 80 };
            var usernameTextBox = new TextBox { Left = 110, Top = 30, Width = 170 };

            var passwordLabel = new Label { Text = "Password:", Left = 20, Top = 70, Width = 80 };
            var passwordTextBox = new TextBox { Left = 110, Top = 70, Width = 170, PasswordChar = '*' };

            var loginButton = new Button { Text = "Login", Left = 110, Top = 120, Width = 80 };
            loginButton.Click += async (sender, e) => await Login(usernameTextBox.Text, passwordTextBox.Text);

            var registerLabel = new Label { Text = "Don't have an account?", Left = 20, Top = 160, Width = 120 };
            var registerLink = new LinkLabel { Text = "Register here", Left = 140, Top = 160, Width = 80 };
            registerLink.Click += (sender, e) => ShowRegisterDialog();

            loginPanel.Controls.AddRange(new Control[] { usernameLabel, usernameTextBox,
                passwordLabel, passwordTextBox, loginButton, registerLabel, registerLink });
        }

        private void InitializeBorrowersTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            _borrowersTab.Controls.Add(panel);

            var dataGridView = new DataGridView
            {
                Name = "borrowersGrid",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            panel.Controls.Add(dataGridView);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(buttonPanel);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Left,
                Width = 100
            };
            refreshButton.Click += async (sender, e) => await LoadBorrowers();
            buttonPanel.Controls.Add(refreshButton);

            var addButton = new Button
            {
                Text = "Add Borrower",
                Dock = DockStyle.Left,
                Width = 100
            };
            addButton.Click += ShowAddBorrowerDialog;
            buttonPanel.Controls.Add(addButton);

            var editButton = new Button
            {
                Text = "Edit Borrower",
                Dock = DockStyle.Left,
                Width = 100
            };
            editButton.Click += ShowEditBorrowerDialog;
            buttonPanel.Controls.Add(editButton);

            var deleteButton = new Button
            {
                Text = "Delete Borrower",
                Dock = DockStyle.Left,
                Width = 100
            };
            deleteButton.Click += DeleteBorrower;
            buttonPanel.Controls.Add(deleteButton);
        }

        private async Task LoadBorrowers()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                MessageBox.Show("Please log in to view borrowers.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var response = await _httpClient.GetAsync("borrowers");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var borrowers = JsonSerializer.Deserialize<List<BorrowerDTO>>(content);

                var grid = (DataGridView)_borrowersTab.Controls[0].Controls["borrowersGrid"];
                grid.DataSource = borrowers;
                grid.Columns["BorrowerId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading borrowers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAddBorrowerDialog(object sender, EventArgs e)
        {
            var form = new Form
            {
                Text = "Add New Borrower",
                Width = 400,
                Height = 250,
                StartPosition = FormStartPosition.CenterParent
            };

            var nameLabel = new Label { Text = "Name:", Left = 20, Top = 20, Width = 100 };
            var nameTextBox = new TextBox { Left = 130, Top = 20, Width = 200 };

            var emailLabel = new Label { Text = "Email:", Left = 20, Top = 60, Width = 100 };
            var emailTextBox = new TextBox { Left = 130, Top = 60, Width = 200 };

            var phoneLabel = new Label { Text = "Phone:", Left = 20, Top = 100, Width = 100 };
            var phoneTextBox = new TextBox { Left = 130, Top = 100, Width = 200 };

            var saveButton = new Button { Text = "Save", Left = 130, Top = 140, Width = 80 };
            saveButton.Click += async (s, ev) =>
            {
                var newBorrower = new BorrowerCreateDTO
                {
                    Name = nameTextBox.Text,
                    Email = emailTextBox.Text,
                    Phone = phoneTextBox.Text
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(newBorrower), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("borrowers", content);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Borrower added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    await LoadBorrowers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding borrower: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var cancelButton = new Button { Text = "Cancel", Left = 220, Top = 140, Width = 80 };
            cancelButton.Click += (s, ev) => form.Close();

            form.Controls.AddRange(new Control[] { nameLabel, nameTextBox,
                emailLabel, emailTextBox, phoneLabel, phoneTextBox,
                saveButton, cancelButton });

            form.ShowDialog();
        }

        private void ShowEditBorrowerDialog(object sender, EventArgs e)
        {
            var grid = (DataGridView)_borrowersTab.Controls[0].Controls["borrowersGrid"];
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a borrower to edit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedBorrower = (BorrowerDTO)grid.SelectedRows[0].DataBoundItem;
            var form = new Form
            {
                Text = "Edit Borrower",
                Width = 400,
                Height = 250,
                StartPosition = FormStartPosition.CenterParent
            };

            var nameLabel = new Label { Text = "Name:", Left = 20, Top = 20, Width = 100 };
            var nameTextBox = new TextBox { Left = 130, Top = 20, Width = 200, Text = selectedBorrower.Name };

            var emailLabel = new Label { Text = "Email:", Left = 20, Top = 60, Width = 100 };
            var emailTextBox = new TextBox { Left = 130, Top = 60, Width = 200, Text = selectedBorrower.Email };

            var phoneLabel = new Label { Text = "Phone:", Left = 20, Top = 100, Width = 100 };
            var phoneTextBox = new TextBox { Left = 130, Top = 100, Width = 200, Text = selectedBorrower.Phone };

            var saveButton = new Button { Text = "Save", Left = 130, Top = 140, Width = 80 };
            saveButton.Click += async (s, ev) =>
            {
                var updatedBorrower = new BorrowerUpdateDTO
                {
                    Name = nameTextBox.Text,
                    Email = emailTextBox.Text,
                    Phone = phoneTextBox.Text
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(updatedBorrower), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PutAsync($"borrowers/{selectedBorrower.BorrowerId}", content);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Borrower updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    await LoadBorrowers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating borrower: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var cancelButton = new Button { Text = "Cancel", Left = 220, Top = 140, Width = 80 };
            cancelButton.Click += (s, ev) => form.Close();

            form.Controls.AddRange(new Control[] { nameLabel, nameTextBox,
                emailLabel, emailTextBox, phoneLabel, phoneTextBox,
                saveButton, cancelButton });

            form.ShowDialog();
        }

        private async void DeleteBorrower(object sender, EventArgs e)
        {
            var grid = (DataGridView)_borrowersTab.Controls[0].Controls["borrowersGrid"];
            if (grid.SelectedRows.Count == 0) return;

            var borrower = (BorrowerDTO)grid.SelectedRows[0].DataBoundItem;

            if (MessageBox.Show($"Delete {borrower.Name}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"borrowers/{borrower.BorrowerId}");
                    response.EnsureSuccessStatusCode();
                    await LoadBorrowers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting borrower: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeLoansTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            _loansTab.Controls.Add(panel);

            var tabControl = new TabControl { Dock = DockStyle.Fill };
            panel.Controls.Add(tabControl);

            // Current Loans tab
            var currentLoansTab = new TabPage("Current Loans");
            InitializeCurrentLoansTab(currentLoansTab);
            tabControl.TabPages.Add(currentLoansTab);

            // Overdue Loans tab
            var overdueLoansTab = new TabPage("Overdue Loans");
            InitializeOverdueLoansTab(overdueLoansTab);
            tabControl.TabPages.Add(overdueLoansTab);

            // Issue/Return tab
            var manageLoansTab = new TabPage("Manage Loans");
            InitializeManageLoansTab(manageLoansTab);
            tabControl.TabPages.Add(manageLoansTab);
        }

        private void InitializeCurrentLoansTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            tab.Controls.Add(panel);

            var dataGridView = new DataGridView
            {
                Name = "currentLoansGrid",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            panel.Controls.Add(dataGridView);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(buttonPanel);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Left,
                Width = 100
            };
            refreshButton.Click += async (sender, e) => await LoadCurrentLoans();
            buttonPanel.Controls.Add(refreshButton);
        }

        private async Task LoadCurrentLoans()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                MessageBox.Show("Please log in to view current loans.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var response = await _httpClient.GetAsync("loans");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var loans = JsonSerializer.Deserialize<List<LoanDTO>>(content);

                var grid = (DataGridView)_loansTab.Controls[0].Controls[0].Controls["currentLoansGrid"];
                grid.DataSource = loans.Where(l => !l.ReturnDate.HasValue).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading current loans: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeOverdueLoansTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            tab.Controls.Add(panel);

            var dataGridView = new DataGridView
            {
                Name = "overdueLoansGrid",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            panel.Controls.Add(dataGridView);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(buttonPanel);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Left,
                Width = 100
            };
            refreshButton.Click += async (sender, e) => await LoadOverdueLoans();
            buttonPanel.Controls.Add(refreshButton);
        }

        private async Task LoadOverdueLoans()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                MessageBox.Show("Please log in to view overdue loans.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                // Assuming your API has an endpoint for overdue loans, e.g., "loans/overdue"
                // You might need to adjust this endpoint based on your API's implementation
                var response = await _httpClient.GetAsync("loans/overdue");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var loans = JsonSerializer.Deserialize<List<LoanDTO>>(content);

                var grid = (DataGridView)_loansTab.Controls[0].Controls[0].Controls["overdueLoansGrid"];
                // Filter overdue loans client-side if API doesn't provide a specific endpoint
                grid.DataSource = loans.Where(l => !l.ReturnDate.HasValue && l.DueDate < DateTime.Now).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading overdue loans: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeManageLoansTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            tab.Controls.Add(panel);

            var bookLabel = new Label { Text = "Book:", Left = 20, Top = 20, Width = 100 };
            var bookComboBox = new ComboBox { Left = 130, Top = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            bookComboBox.Name = "bookComboBox"; // Name for easier access

            var borrowerLabel = new Label { Text = "Borrower:", Left = 20, Top = 60, Width = 100 };
            var borrowerComboBox = new ComboBox { Left = 130, Top = 60, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            borrowerComboBox.Name = "borrowerComboBox"; // Name for easier access

            var issueButton = new Button { Text = "Issue Loan", Left = 130, Top = 100, Width = 100 };
            issueButton.Click += async (sender, e) => await IssueLoan(bookComboBox.SelectedItem as BookDTO, borrowerComboBox.SelectedItem as BorrowerDTO);

            var returnButton = new Button { Text = "Return Loan", Left = 240, Top = 100, Width = 100 };
            returnButton.Click += async (sender, e) => await ReturnLoan();

            panel.Controls.AddRange(new Control[] { bookLabel, bookComboBox, borrowerLabel, borrowerComboBox, issueButton, returnButton });

            // Load combo box data when tab is selected
            _loansTab.Enter += async (s, e) => await LoadLoanManagementData();
        }

        private async Task LoadLoanManagementData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                MessageBox.Show("Please log in to manage loans.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                // Load books for the combo box
                var booksResponse = await _httpClient.GetAsync("books");
                booksResponse.EnsureSuccessStatusCode();
                var booksContent = await booksResponse.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<BookDTO>>(booksContent);

                var bookComboBox = (ComboBox)_loansTab.Controls[0].Controls["bookComboBox"];
                bookComboBox.DataSource = books;
                bookComboBox.DisplayMember = "Title";
                bookComboBox.ValueMember = "BookId";

                // Load borrowers for the combo box
                var borrowersResponse = await _httpClient.GetAsync("borrowers");
                borrowersResponse.EnsureSuccessStatusCode();
                var borrowersContent = await borrowersResponse.Content.ReadAsStringAsync();
                var borrowers = JsonSerializer.Deserialize<List<BorrowerDTO>>(borrowersContent);

                var borrowerComboBox = (ComboBox)_loansTab.Controls[0].Controls["borrowerComboBox"];
                borrowerComboBox.DataSource = borrowers;
                borrowerComboBox.DisplayMember = "Name";
                borrowerComboBox.ValueMember = "BorrowerId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loan management data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task IssueLoan(BookDTO selectedBook, BorrowerDTO selectedBorrower)
        {
            if (selectedBook == null || selectedBorrower == null)
            {
                MessageBox.Show("Please select a book and a borrower.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var issueLoanDto = new LoanCreateDTO
            {
                BookId = selectedBook.BookId,
                BorrowerId = selectedBorrower.BorrowerId,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14) // Example: due in 14 days
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(issueLoanDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("loans", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Loan issued successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCurrentLoans(); // Refresh current loans list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error issuing loan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ReturnLoan()
        {
            // You'll need to select a loan from the current loans grid to return it.
            // This example assumes you'd have a way to get the LoanId to return.
            // For a more complete UI, you might have a dedicated button per row, or
            // a selected row in the current loans grid.
            var currentLoansGrid = (DataGridView)_loansTab.Controls[0].Controls[0].Controls["currentLoansGrid"];
            if (currentLoansGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a loan to return from the 'Current Loans' tab.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedLoan = (LoanDTO)currentLoansGrid.SelectedRows[0].DataBoundItem;

            if (MessageBox.Show($"Return loan for '{selectedLoan.BookTitle}' by '{selectedLoan.BorrowerName}'?", "Confirm Return", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    // Assuming your API has a PUT or POST endpoint for returning loans, e.g., "loans/return/{id}" or "loans/{id}/return"
                    var returnLoanDto = new LoanReturnDTO { LoanId = selectedLoan.LoanId, ReturnDate = DateTime.Now }; // Example DTO
                    var content = new StringContent(JsonSerializer.Serialize(returnLoanDto), Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync($"loans/return/{selectedLoan.LoanId}", content); // Adjust endpoint if needed
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Loan returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadCurrentLoans(); // Refresh current loans list
                    await LoadOverdueLoans(); // Refresh overdue loans list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error returning loan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

      
    }
}
