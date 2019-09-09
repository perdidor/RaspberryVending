using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace VendingServerInitialSetup
{
    public partial class VendingEntities: DbContext
    {
        public VendingEntities(String connectionString) : base(connectionString)
        {

        }
    }
}
