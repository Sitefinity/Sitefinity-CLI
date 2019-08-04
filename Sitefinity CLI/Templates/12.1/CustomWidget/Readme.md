# Custom Widget Templates

## Default will create the necessary files under the MVC folder for a widget that has ONE property of type String in the Model and Controller to help with the creation of the boiler plate generated code needed for the all aspects of getting the Widget to work and function correctly

### files created for default template:
- Model/{widget}model.cs
- Controllers/{widget}controller.cs
- Scripts/{widget}/designer-view-simple.js
- Views/{widget}/index.cshtml
- Views/{widget}/DesignerView.Simple.cshtml

## MultiTypeProp will create the necessary files under the MVC folder in order to implement several properties of type string, integer, boolean, datetime and enum. This template will generate the code needed for the AngularJS application to persist the value of these properties in the custom designer.  It will also add the proper components in the json file to allow for the KendoUI DateTime Picker to display correctly inthe designer.

### files created for MultiTypeProp template:
- Model/{widget}model.cs
- Controllers/{widget}controller.cs
- Scripts/{widget}/designer-view-simple.js
- Views/{widget}/index.cshtml
- Views/{widget}/DesignerView.Simple.cshtml
- Views/{widget}/DesignerView.Simple.json