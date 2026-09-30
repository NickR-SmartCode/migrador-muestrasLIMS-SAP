class Utils {
   static getCurrencySymbol (code) {
        const symbols = {
            'SOL': 'S/',
            'USD': '$',
            'EUR': '€',
            'PEN': 'S/'
        };
        return symbols[code] || code;
    };
     static getCurrencySymbolByCurName (curName) {
        const symbols = {
            'SOLES': 'S/',
            'DOLARES': '$',
            'EUROS': '€',
            'PEN': 'S/'
        };
        return symbols[curName] || code;
    };

    static getNumFormattedByCurName(amount, curName) {

         switch(curName) {
            case "SOLES": {
                const fmt = new Intl.NumberFormat('es-PE', { 
            style: 'currency', 
            currency: 'PEN', 
            minimumFractionDigits: 2 
                });
                return  fmt.format(amount)
            }
            case "DOLARES": {
                const fmt =   new Intl.NumberFormat('es-US', { 
            style: 'currency', 
            currency: 'USD', 
            minimumFractionDigits: 2 
                });
                return fmt.format(amount);Estado
            }
         }
    }
}