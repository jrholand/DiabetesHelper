using DiabetesHelper.Models;
using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class EditInsulinDosePage : ContentPage
{
    private readonly EditInsulinDoseViewModel _viewModel;

    public EditInsulinDosePage(EditInsulinDoseViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void Load(InsulinDose record) => _viewModel.Load(record);
}
