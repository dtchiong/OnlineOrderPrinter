﻿using OnlineOrderPrinter.Models;
using OnlineOrderPrinter.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace OnlineOrderPrinter.UserControls.Main.Tabs.Orders {
    public partial class UserControlDetailedOrderView : UserControl {

        public UserControlDetailedOrderView() {
            InitializeComponent();
            ConfigureItemListDataGridView();
            ConfigureModifierListDataGridView();
            AppState.UserControlDetailedOrderView = this;
        }

        public void HandleSelectedEventChanged(Event @event) {
            UpdateOrderDetails(@event);
            UpdateItemList(@event?.Order);
        }

        private void ConfigureItemListDataGridView() {
            dataGridViewItemList.AutoGenerateColumns = false;
        }

        private void ConfigureModifierListDataGridView() {
            modifierListDataGridView.AutoGenerateColumns = false;
        }

        private void UpdateOrderDetails(Event @event) {
            labelNameValue.Text = @event?.Order?.CustomerName;
            labelContactValue.Text = @event?.Order?.ContactNumber;
            labelOrderSizeValue.Text = @event?.Order?.OrderSize.ToString();
            label4.Text = @event?.Order?.GmailMessageId;
            label5.Text = @event?.Order?.Id;
        }

        private void UpdateItemList(Order order) {
            dataGridViewItemList.DataSource = order?.OrderItems;
        }

        private void dataGridViewItemList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            DataGridView grid = (DataGridView)sender;
            DataGridViewRow row = grid.Rows[e.RowIndex];
            DataGridViewColumn col = grid.Columns[e.ColumnIndex];

            if (row.DataBoundItem == null) {
                return;
            }

            if (col.DataPropertyName == itemNameDataGridViewTextBoxColumn.DataPropertyName) {
                e.Value = FormatItemName((OrderItem)row.DataBoundItem);
            }
        }

        private string FormatItemName(OrderItem orderItem) {
            if (orderItem.Name != null) {
                return orderItem.Name;
            }

            Order order = AppState.UserControlOrdersView.GetCurrentSelectedEvent()?.Order;

            if (order != null) {
                switch (order.Service) {
                    case ServiceType.DoorDash:
                        return orderItem.DoordashName;
                    case ServiceType.Grubhub:
                        return orderItem.GrubhubName;
                    case ServiceType.UberEats:
                        return orderItem.UbereatsName;
                }
            }
            return "";
        }

        private void dataGridViewItemList_SelectionChanged(object sender, EventArgs e) {
            DataGridViewSelectedRowCollection selectedRows = dataGridViewItemList.SelectedRows;
            OrderItem orderItem = null;

            if (selectedRows.Count > 0) {
                orderItem = (OrderItem)selectedRows[0].DataBoundItem;
            }
            HandleSelectedItemChanged(orderItem);
        }

        private void HandleSelectedItemChanged(OrderItem orderItem) {
            UpdateModifierList(orderItem);
            UpdateSpecialInstructions(orderItem);
        }

        private void UpdateModifierList(OrderItem orderItem) {
            modifierListDataGridView.DataSource = orderItem?.OrderItemModifiers;
        }

        private void UpdateSpecialInstructions(OrderItem orderItem) {
            textBoxSpecialInstructions.Text = orderItem?.SpecialInstructions;
        }

        private void modifierListDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            DataGridView grid = (DataGridView)sender;
            DataGridViewRow row = grid.Rows[e.RowIndex];
            DataGridViewColumn col = grid.Columns[e.ColumnIndex];

            if (row.DataBoundItem == null) {
                return;
            }

            if (col.DataPropertyName == modifierNameDataGridViewTextBoxColumn.DataPropertyName) {
                e.Value = FormatModifierItemName((OrderItemModifier)row.DataBoundItem);
            }
        }

        private string FormatModifierItemName(OrderItemModifier orderItemModifier) {
            if (orderItemModifier.Name != null) {
                return orderItemModifier.Name;
            }

            Order order = AppState.UserControlOrdersView.GetCurrentSelectedEvent()?.Order;

            if (order != null) {
                switch (order.Service) {
                    case ServiceType.DoorDash:
                        return orderItemModifier.DoordashName;
                    case ServiceType.Grubhub:
                        return orderItemModifier.GrubhubName;
                    case ServiceType.UberEats:
                        return orderItemModifier.UbereatsName;
                }
            }
            return "";
        }
    }
}
