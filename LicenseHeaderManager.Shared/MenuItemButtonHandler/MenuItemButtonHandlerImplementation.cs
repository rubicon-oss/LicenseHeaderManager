/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.UpdateViewModels;
using Window = System.Windows.Window;

namespace LicenseHeaderManager.MenuItemButtonHandler
{
  /// <summary>
  ///   Acts as a type work to be done by a <see cref="IMenuItemButtonHandler" /> instance can be delegated to.
  /// </summary>
  public abstract class MenuItemButtonHandlerImplementation
  {
    internal const string UnsupportedOverload =
        "An implementation has not been provided because the operation cannot be performed in a meaningful way. Use another overload.";

    /// <summary>
    ///   Gets a description of the current <see cref="MenuItemButtonHandlerImplementation" /> instance that is presented to the user
    ///   in case of errors.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    ///   Carries out the work that the corresponding <see cref="IMenuItemButtonHandler" /> instance wants to delegate to this
    ///   <see cref="MenuItemButtonHandlerImplementation" /> instance.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <param name="viewModel">
    ///   The view model this implementation needs to update in order to reflect process in the corresponding
    ///   dialog window.
    /// </param>
    /// <param name="solution">The <see cref="Solution" /> instance the work to be done is related with.</param>
    /// <param name="window">
    ///   The <see cref="System.Windows.Window" /> that is on top of the screen while the work to be done is
    ///   being carried out.
    /// </param>
    /// <remarks>
    ///   If not both a Solution and a Window are strictly required for the desired work to be done (even though one or both of
    ///   them might even be
    ///   present, i. e. could theoretically be supplied), use another overload, if possible. If this is not possible, a
    ///   suitable
    ///   overload that does not expect the superfluous parameters is called by the actual implementation.
    /// </remarks>
    public abstract Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel, Solution solution, Window window);

    /// <summary>
    ///   Carries out the work that the corresponding <see cref="IMenuItemButtonHandler" />  instance wants to delegate to this
    ///   <see cref="MenuItemButtonHandlerImplementation" /> instance.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <param name="viewModel">
    ///   The view model this implementation needs to update in order to reflect process in the corresponding
    ///   dialog window.
    /// </param>
    /// <param name="solution">The <see cref="Solution" /> instance the work to be done is related with.</param>
    /// <remarks>
    ///   If no Solution is strictly required for the desired work to be done (even though one might be present, i. e. could
    ///   theoretically be supplied),
    ///   use another overload, if possible. If this is not possible, a suitable overload that does not expect the superfluous
    ///   parameters is called by
    ///   the actual implementation.
    /// </remarks>
    public abstract Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel, Solution solution);

    /// <summary>
    ///   Carries out the work that the corresponding <see cref="IMenuItemButtonHandler" />
    ///   instance wants to delegate to this <see cref="MenuItemButtonHandlerImplementation" /> instance.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <param name="viewModel">
    ///   The view model this implementation needs to update in order to reflect process in the corresponding
    ///   dialog window.
    /// </param>
    public abstract Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel);
  }
}
