using DiabetesHelper.Models;
using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class EditMealPage : ContentPage
{
    private readonly EditMealViewModel _viewModel;

    public EditMealPage(EditMealViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void Load(Meal record) => _viewModel.Load(record);
}
