using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace Application.Dtos
{
    public class PizzaCreateRequest
    {
        public PizzaDto PizzaDto { get; set; } = new();
        public IBrowserFile? Image { get; set; }
    }
}
