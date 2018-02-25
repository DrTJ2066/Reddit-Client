﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace RedditClient.Controls
{
    public class RepeaterLayout : StackLayout
    {
        #region Fields

        private ScrollView scrollView;
        private StackLayout contentLayout;
        private StackLayout pullToRefreshLayout;
        private bool isPullingToRefresh;

        #endregion

        #region Constructors

        public RepeaterLayout()
        {
            IsPullingToRefresh = false;

            contentLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            scrollView = new ScrollView()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = contentLayout
            };
            scrollView.Scrolled += ScrollView_Scrolled;

            #region Pull to refresh layouts

            pullToRefreshLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                IsVisible = false,
            };

            var refreshImage = new Image()
            {
                Source = "refresh32.png",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center, 
            };

            pullToRefreshLayout.Children.Add(refreshImage);

            #endregion

            var gridView = new Grid()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                RowDefinitions = new RowDefinitionCollection() { new RowDefinition() },
                ColumnDefinitions = new ColumnDefinitionCollection() { new ColumnDefinition() }            
            };

			gridView.Children.Add(pullToRefreshLayout, 0, 0);
            gridView.Children.Add(scrollView, 0, 0);

            Children.Add(gridView);
        }

        #endregion

        #region Bindable Properties

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList),
            typeof(RepeaterLayout), 
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var control = bindable as RepeaterLayout;
                control.ItemsSource = (IList)newValue;
                control.RebuildList();
            }
        );

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(RepeaterLayout), 
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var control = bindable as RepeaterLayout;
                control.RebuildList();
            }
        );

        public static readonly BindableProperty ItemTappedCommandProperty = BindableProperty.Create(
            nameof(ItemTappedCommand),
            typeof(Command<object>),
            typeof(RepeaterLayout)
        );

        public static readonly BindableProperty PullToRefreshCommandProperty = BindableProperty.Create(
            nameof(PullToRefreshCommand),
            typeof(Command),
            typeof(RepeaterLayout)
        );

        public static readonly BindableProperty IsPullToRefreshEnabledProperty = BindableProperty.Create(
            nameof(IsPullToRefreshEnabled),
            typeof(bool),
            typeof(RepeaterLayout),
            false
        );


        #endregion

        #region Properties

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set { SetValue(ItemTemplateProperty, value); }
        }

        public Command<object> ItemTappedCommand
        {
            get => (Command<object>)GetValue(ItemTappedCommandProperty);
            set { SetValue(ItemTappedCommandProperty, value); }
        }

        public Command PullToRefreshCommand
        {
            get => (Command)GetValue(PullToRefreshCommandProperty);
            set { SetValue(PullToRefreshCommandProperty, value); }
        }

        public bool IsPullToRefreshEnabled
        {
            get => (bool)GetValue(IsPullToRefreshEnabledProperty);
            set { SetValue(IsPullToRefreshEnabledProperty, value); }
        }

        public bool IsPullingToRefresh
        {
            get => isPullingToRefresh;
            set 
            {
                isPullingToRefresh = value;

                if (pullToRefreshLayout != null)
                {
                    pullToRefreshLayout.IsVisible = value;
                }
            }
        }

        #endregion

        #region Methods

        public void RebuildList()
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                this.contentLayout.Children.Clear();
                return;
            }

            foreach (var item in ItemsSource)
            {
                var view = this.GenerateItemLayout(item);

                if (view != null)
                {
                    this.contentLayout.Children.Add(view);
                }
            }
        }

        private View GenerateItemLayout(object context)
        {
            ViewCell content = (ViewCell)this.ItemTemplate.CreateContent();
            View view = content?.View;

            if (view == null)
            {
                return null;
            }

            view.BindingContext = context;

            if(ItemTappedCommand != null)
            {
				view.GestureRecognizers.Add(new TapGestureRecognizer() { Command = ItemTappedCommand });
            }

            return view;
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if(e.ScrollY <= -50 && !IsPullingToRefresh)
            {
                IsPullingToRefresh = true;

                // The user is pulling to refresh
                if(PullToRefreshCommand == null || !IsPullToRefreshEnabled || !PullToRefreshCommand.CanExecute(null))
                {
                    return;
                }

                PullToRefreshCommand.Execute(null);
            }

            if(e.ScrollY >= 0 && IsPullingToRefresh)
            {
                IsPullingToRefresh = false;
            }
        }
		
        #endregion
    }
}
