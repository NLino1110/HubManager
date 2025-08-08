using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ResourceBuilder.Shared.Modal
{
    public class EmailSettings
    {
        /*
        CREATE TABLE email_Settings (
  id int(11) NOT NULL,
  tax_id varchar(13) NOT NULL,
  host varchar(50) DEFAULT NULL,
  port int(11) DEFAULT NULL,
  userName varchar(45) DEFAULT NULL,
  password varchar(45) DEFAULT NULL,
  EnableSsl bit(1) DEFAULT NULL,
  UseDefaultCredentials bit(1) DEFAULT NULL,
  IsActive bit(1) DEFAULT NULL,
  PRIMARY KEY (tax_id,id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        */
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int id { get; set; }
        public string tax_id { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public bool IsActive { get; set; }
    }
}
