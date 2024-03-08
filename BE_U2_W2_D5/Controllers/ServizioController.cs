using BE_U2_W2_D5.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace BE_U2_W2_D5.Controllers
{
    public class ServizioController : Controller
    {
        public ActionResult ElencoServizi()
        {
            List<Servizio> serviziClienti = new List<Servizio>();

            using (SqlConnection con = Connessioni.GetConnection())
            {
                con.Open();
                string query = @"
            SELECT s.IdServizio, s.TipoServizio, s.Quantita, s.PrezzoTot, s.DataAggiunta, c.Nome, c.Cognome 
            FROM Servizio s
            INNER JOIN Prenotazione p ON s.IdPrenotazione = p.IdPrenotazione
            INNER JOIN Cliente c ON p.IdCliente = c.IdCliente";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serviziClienti.Add(new Servizio
                            {
                                IdServizio = int.Parse(reader["IdServizio"].ToString()),
                                TipoServizio = reader["TipoServizio"].ToString(),
                                Quantita = int.Parse(reader["Quantita"].ToString()),
                                PrezzoTot = decimal.Parse(reader["PrezzoTot"].ToString()),
                                DataAggiunta = DateTime.Parse(reader["DataAggiunta"].ToString()),

                            });
                        }
                    }
                }
            }

            return View(serviziClienti);
        }

        // GET: Metodo per visualizzare il form di aggiunta servizio
        public ActionResult AggiungiServizio()
        {
            List<SelectListItem> clientiList = new List<SelectListItem>();
            List<SelectListItem> serviziList = new List<SelectListItem>();

            using (SqlConnection con = Connessioni.GetConnection())
            {
                con.Open();

                // Recupera tutti i clienti
                string queryClienti = "SELECT IdCliente, Nome, Cognome FROM Cliente";
                using (SqlCommand cmd = new SqlCommand(queryClienti, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientiList.Add(new SelectListItem
                            {
                                Value = reader["IdCliente"].ToString(),
                                Text = $"{reader["Cognome"]} {reader["Nome"]}"
                            });
                        }
                    }
                }

                // Recupera tutti i servizi
                string queryServizi = "SELECT IdServizio, TipoServizio FROM Servizio";
                using (SqlCommand cmd = new SqlCommand(queryServizi, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serviziList.Add(new SelectListItem
                            {
                                Value = reader["IdServizio"].ToString(),
                                Text = reader["TipoServizio"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Clienti = clientiList;
            ViewBag.Servizi = serviziList;
            return View();
        }



        // POST: Metodo per aggiungere un servizio a una prenotazione
        [HttpPost]
        public ActionResult AggiungiServizio(Servizio servizio)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = Connessioni.GetConnection())
                {
                    string comandoSql = "INSERT INTO Servizio (TipoServizio, Quantita, PrezzoTot, DataAggiunta, IdPrenotazione) VALUES (@TipoServizio, @Quantita, @PrezzoTot, @DataAggiunta, @IdPrenotazione)";

                    using (SqlCommand cmd = new SqlCommand(comandoSql, con))
                    {
                        cmd.Parameters.AddWithValue("@TipoServizio", servizio.TipoServizio);
                        cmd.Parameters.AddWithValue("@Quantita", servizio.Quantita);
                        cmd.Parameters.AddWithValue("@PrezzoTot", servizio.PrezzoTot);
                        cmd.Parameters.AddWithValue("@DataAggiunta", servizio.DataAggiunta == null ? DateTime.Now : servizio.DataAggiunta);
                        cmd.Parameters.AddWithValue("@IdPrenotazione", servizio.IdPrenotazione);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index"); // Reindirizza alla vista principale o a una vista di conferma
            }

            // Ricarica la lista delle prenotazioni in caso di errore di validazione
            List<SelectListItem> prenotazioniList = new List<SelectListItem>();
            using (SqlConnection con = Connessioni.GetConnection())
            {
                con.Open();
                string query = "SELECT Prenotazioni.IdPrenotazione, Clienti.Nome, Clienti.Cognome FROM Prenotazioni INNER JOIN Clienti ON Prenotazioni.IdCliente = Clienti.IdCliente";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prenotazioniList.Add(new SelectListItem
                            {
                                Value = reader["IdPrenotazione"].ToString(),
                                Text = $"{reader["Cognome"]} {reader["Nome"]} - {reader["IdPrenotazione"]}"
                            });
                        }
                    }
                }
            }

            ViewBag.IdPrenotazione = prenotazioniList;
            return View(servizio); // Ritorna alla vista di inserimento se il modello non è valido
        }


    }
}