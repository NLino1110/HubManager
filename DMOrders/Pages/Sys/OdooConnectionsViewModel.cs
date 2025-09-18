using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Sys
{
    public class OdooConnectionsViewModel : BindableObject
    {
        private OdooConnectionDb _database;

        public OdooConnectionsViewModel()
        {
            _database = new OdooConnectionDb();

            LoadConnections();

            // Comandos
            SaveCommand = new Command(async () => await SaveConnection());
            NewCommand = new Command(NewConnection);
            DeleteCommand = new Command(async () => await DeleteConnection());
        }

        private ObservableCollection<OdooConnection> _connections;
        public ObservableCollection<OdooConnection> Connections
        {
            get => _connections;
            set
            {
                _connections = value;
                OnPropertyChanged();
            }
        }

        private OdooConnection _selectedConnection = new OdooConnection();
        public OdooConnection SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                _selectedConnection = value ?? new OdooConnection();
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Comandos
        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand DeleteCommand { get; }

        // Cargar conexiones desde DB
        private async void LoadConnections()
        {
            var items = await _database.GetItemsAsync();
            Connections = new ObservableCollection<OdooConnection>(items);
        }

        // Nuevo registro
        private void NewConnection()
        {
            SelectedConnection = new OdooConnection();
        }

        // Guardar (insert/update)
        private async System.Threading.Tasks.Task SaveConnection()
        {
            try
            {
                if (SelectedConnection.Id == 0)
                {
                    if (SelectedConnection.Password != string.Empty)
                    {
                        try
                        {
                            string decrypt = CryptoHelper.Decrypt(SelectedConnection.Password);
                            Debug.WriteLine(decrypt);
                        }
                        catch (FormatException fe)
                        {
                            Console.WriteLine("Error:" + fe.Message);
                            //Aquí se detecta que no es un texto encriptado, es decir que el valor fue modificado
                            // y se requiere volver a encryptar
                            // ya que la idea es que no se muestre desencriptado en el mantenimiento
                            // solo será desencriptado cuando se use para la conexión
                            SelectedConnection.Password = CryptoHelper.Encrypt(SelectedConnection.Password);
                        }
                        catch (CryptographicException ex)
                        {
                            Console.WriteLine("Error al descifrar: " + ex.Message);
                            SelectedConnection.Password = CryptoHelper.Encrypt(SelectedConnection.Password);
                        }
                    }

                    await _database.InsertAsync(SelectedConnection);
                    StatusMessage = "Connection added successfully.";
                }
                else
                {
                    await _database.UpdateAsync(SelectedConnection);
                    StatusMessage = "Connection updated successfully.";
                }
                LoadConnections();
                NewConnection();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        // Eliminar
        private async System.Threading.Tasks.Task DeleteConnection()
        {
            if (SelectedConnection?.Id != 0)
            {
                try
                {
                    //await _database.DeleteAsync(SelectedConnection);
                    StatusMessage = "Connection deleted successfully.";
                    LoadConnections();
                    NewConnection();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            }
            else
            {
                StatusMessage = "Select a connection to delete.";
            }
        }
    }
}
